namespace TuColmadoRD.Presentation.API.Endpoints.Customers;

public sealed record CreateCustomerAddressDto(
    string Province,
    string Sector,
    string Street,
    string Reference,
    string? HouseNumber);

public sealed record CreateCustomerRequest(
    string FullName,
    string DocumentId,
    string? Phone,
    CreateCustomerAddressDto? Address,
    decimal? CreditLimit);

public sealed record CreateCustomerResponse(
    Guid CustomerId,
    Guid AccountId,
    decimal Balance,
    decimal CreditLimit,
    bool IsActive,
    DateTime CreatedAt);

public sealed record RegisterPaymentRequest(
    decimal Amount,
    int PaymentMethodId,
    string Concept);
