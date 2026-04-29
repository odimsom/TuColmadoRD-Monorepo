using MediatR;
using TuColmadoRD.Core.Application.Customers.Commands;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, OperationResult<CreateCustomerResult, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ITenantProvider tenantProvider,
        ICustomerRepository customerRepository,
        ICustomerAccountRepository customerAccountRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _customerRepository = customerRepository;
        _customerAccountRepository = customerAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<CreateCustomerResult, DomainError>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var documentResult = Cedula.Create(request.DocumentId);
        if (!documentResult.TryGetResult(out var document) || document is null)
        {
            return OperationResult<CreateCustomerResult, DomainError>.Bad(
                DomainError.Validation("customer.document_invalid", documentResult.Error));
        }

        var existing = await _customerRepository.GetByDocumentIdAsync(document.Value, cancellationToken);
        if (existing is not null)
        {
            return OperationResult<CreateCustomerResult, DomainError>.Bad(
                DomainError.Business("customer.already_exists", "Ya existe un cliente con esa cedula."));
        }

        Phone? phone = null;
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneResult = Phone.Create(request.Phone);
            if (!phoneResult.TryGetResult(out phone) || phone is null)
            {
                return OperationResult<CreateCustomerResult, DomainError>.Bad(
                    DomainError.Validation("customer.phone_invalid", phoneResult.Error));
            }
        }

        Address? address = null;
        if (request.Address is not null)
        {
            var addressResult = Address.Create(
                request.Address.Province,
                request.Address.Sector,
                request.Address.Street,
                request.Address.Reference,
                request.Address.HouseNumber,
                request.Address.Latitude,
                request.Address.Longitude);

            if (!addressResult.TryGetResult(out address) || address is null)
            {
                return OperationResult<CreateCustomerResult, DomainError>.Bad(
                    DomainError.Validation("customer.address_invalid", addressResult.Error));
            }
        }

        var createResult = Customer.Create(_tenantProvider.TenantId, request.FullName, document, phone, address);
        if (!createResult.TryGetResult(out var customer) || customer is null)
        {
            return OperationResult<CreateCustomerResult, DomainError>.Bad(
                DomainError.Validation("customer.invalid", createResult.Error));        }

        Money creditLimit = Money.Zero;
        if (request.CreditLimit.HasValue)        {
            var creditResult = Money.FromDecimal(request.CreditLimit.Value);    
            if (!creditResult.TryGetResult(out creditLimit!) || creditLimit is null)
            {
                return OperationResult<CreateCustomerResult, DomainError>.Bad(  
                    DomainError.Validation("customer.credit_invalid", creditResult.Error?.Message));
            }
        }
        else
        {
            creditLimit = Money.FromDecimal(5000).Result!; // Default
        }

        var accountResult = CustomerAccount.Create(_tenantProvider.TenantId, customer.Id, creditLimit);
        if (!accountResult.TryGetResult(out var account) || account is null)
        {
            return OperationResult<CreateCustomerResult, DomainError>.Bad(
                DomainError.Validation("customer_account.invalid", "Limíte de crédito inválido"));
        }

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _customerAccountRepository.AddAsync(account, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<CreateCustomerResult, DomainError>.Good(
            new CreateCustomerResult(
                customer.Id,
                account.Id,
                account.Balance.Amount,
                account.CreditLimit.Amount,
                customer.IsActive,
                customer.CreatedAt));
    }
}
