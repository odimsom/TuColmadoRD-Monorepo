using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Purchasing.Commands;

public sealed record PurchaseItemRequest(Guid ProductId, decimal Quantity, decimal UnitCost);

public sealed record CreateAndCompletePurchaseCommand(
    Guid SupplierId,
    string SupplierNcf,
    IReadOnlyList<PurchaseItemRequest> Items
) : IRequest<OperationResult<Guid, DomainError>>;
