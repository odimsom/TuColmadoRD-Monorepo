using MediatR;
using TuColmadoRD.Core.Application.Customers.Queries;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Customers;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Customers;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Customers.Handlers;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, OperationResult<CustomerDetailResult, DomainError>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerAccountRepository _customerAccountRepository;
    private readonly ITenantProvider _tenantProvider;

    public GetCustomerByIdQueryHandler(
        ICustomerRepository customerRepository,
        ICustomerAccountRepository customerAccountRepository,
        ITenantProvider tenantProvider)
    {
        _customerRepository = customerRepository;
        _customerAccountRepository = customerAccountRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<OperationResult<CustomerDetailResult, DomainError>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, null, cancellationToken);
        if (customer is null || customer.TenantId != _tenantProvider.TenantId)
        {
            return OperationResult<CustomerDetailResult, DomainError>.Bad(
                DomainError.NotFound("customer.not_found", "Cliente no encontrado."));
        }

        var accounts = await _customerAccountRepository.GetAllAsync(null, cancellationToken);
        var account = accounts.FirstOrDefault(a => a.CustomerId == customer.Id && a.TenantId == _tenantProvider.TenantId);

        if (account is null)
        {
            return OperationResult<CustomerDetailResult, DomainError>.Bad(
                DomainError.NotFound("customer_account.not_found", "Cuenta no encontrada para este cliente."));
        }

        var detail = new CustomerDetailResult(
            customer.Id,
            customer.FullName,
            customer.DocumentId?.Value ?? string.Empty,
            customer.ContactPhone?.Value,
            customer.IsActive,
            customer.CreatedAt,
            account.Id,
            account.Balance.Amount,
            account.CreditLimit.Amount,
            account.LastActivity,
            account.Status.ToString());

        return OperationResult<CustomerDetailResult, DomainError>.Good(detail);
    }
}
