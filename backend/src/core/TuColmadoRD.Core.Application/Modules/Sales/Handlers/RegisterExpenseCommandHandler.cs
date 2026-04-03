using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Entities.Treasury;
using TuColmadoRD.Core.Domain.Enums.Treasury;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Treasury;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Sales.Handlers;

public sealed class RegisterExpenseCommandHandler : IRequestHandler<RegisterExpenseCommand, OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly IExpenseRepository _expenseRepository;
    private readonly TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterExpenseCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        IExpenseRepository expenseRepository,
        TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _expenseRepository = expenseRepository;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> Handle(RegisterExpenseCommand request, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(_tenantProvider.TenantId.Value, _tenantProvider.TerminalId, cancellationToken);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(shiftResult.Error ?? DomainError.NotFound("shift.not_found", "No hay un turno abierto en esta caja."));
        }

        var amountResult = Money.FromDecimal(request.Amount);
        if (!amountResult.TryGetResult(out var moneyAmount) || moneyAmount is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Validation("expense.amount_invalid", "Monto del gasto invalido."));
        }

        ExpenseCategory category = ExpenseCategory.Other;
        if (Enum.TryParse<ExpenseCategory>(request.Category, true, out var parsedCategory))
        {
            category = parsedCategory;
        }
        
        var expenseResult = Expense.Record(
            _tenantProvider.TenantId,
            _tenantProvider.TerminalId,
            request.Description,
            moneyAmount,
            category
        );

        if (!expenseResult.IsGood)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Business("expense.record_failed", expenseResult.Error!));
        }

        var expense = expenseResult.Result!;

        var registerExpenseResult = shift.RegisterExpense(moneyAmount);
        if (!registerExpenseResult.IsGood)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(registerExpenseResult.Error!);
        }

        var payload = new 
        {
            ExpenseId = expense.Id,
            TenantId = expense.TenantId.Value,
            CashBoxId = expense.CashBoxId,
            Amount = expense.Amount.Amount,
            Category = expense.Category.ToString(),
            Description = expense.Description,
            Date = expense.Date
        };

        var outboxMessage = new OutboxMessage("ExpenseRegistered", JsonSerializer.Serialize(payload));

        await _expenseRepository.AddAsync(expense, cancellationToken);
        await _shiftRepository.UpdateAsync(shift, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
