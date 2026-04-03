using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TuColmadoRD.Core.Application.Customers.Commands;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Sales;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class RegisterAccountPaymentCommandHandler : IRequestHandler<RegisterAccountPaymentCommand, OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentShiftService _shiftService;
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository _shiftRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterAccountPaymentCommandHandler(
        ITenantProvider tenantProvider,
        ICurrentShiftService shiftService,
        ICustomerAccountRepository customerAccountRepository,
        TuColmadoRD.Core.Domain.Interfaces.Repositories.Sales.IShiftRepository shiftRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _shiftService = shiftService;
        _customerAccountRepository = customerAccountRepository;
        _shiftRepository = shiftRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>> Handle(RegisterAccountPaymentCommand request, CancellationToken cancellationToken)
    {
        var shiftResult = await _shiftService.GetOpenShiftOrFailAsync(_tenantProvider.TenantId.Value, _tenantProvider.TerminalId, cancellationToken);
        if (!shiftResult.TryGetResult(out var shift) || shift is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(shiftResult.Error ?? DomainError.NotFound("shift.not_found", "No hay un turno abierto en esta caja."));
        }

        var paymentMethodResult = PaymentMethod.FromId(request.PaymentMethodId);
        if (!paymentMethodResult.TryGetResult(out var paymentMethod) || paymentMethod is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Validation("payment_method.invalid", "Metodo de pago proporcionado no valido."));
        }

        var account = await _customerAccountRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        if (account is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.NotFound("customer_account.not_found", "No se encontro la cuenta de credito para el cliente indicado."));
        }

        var amountResult = Money.FromDecimal(request.Amount);
        if (!amountResult.TryGetResult(out var moneyAmount) || moneyAmount is null)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Validation("payment.amount_invalid", "Monto del abono invalido."));
        }

        var recordPaymentResult = account.RecordPayment(moneyAmount, _tenantProvider.TerminalId, request.Concept);
        if (!recordPaymentResult.IsGood)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(DomainError.Business("account.payment_rejected", recordPaymentResult.Error!));
        }

        var shiftPaymentResult = shift.RegisterAccountPayment(moneyAmount, paymentMethod);
        if (!shiftPaymentResult.IsGood)
        {
            return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Bad(shiftPaymentResult.Error!);
        }

        foreach (var domainEvent in account.DomainEvents)
        {
            var outboxMessage = new OutboxMessage(domainEvent.GetType().Name, JsonSerializer.Serialize(domainEvent));
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        foreach (var domainEvent in shift.DomainEvents)
        {
            var outboxMessage = new OutboxMessage(domainEvent.GetType().Name, JsonSerializer.Serialize(domainEvent));
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        }

        await _customerAccountRepository.UpdateAsync(account, cancellationToken);
        await _shiftRepository.UpdateAsync(shift, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<TuColmadoRD.Core.Domain.Base.Result.Unit, DomainError>.Good(TuColmadoRD.Core.Domain.Base.Result.Unit.Value);
    }
}
