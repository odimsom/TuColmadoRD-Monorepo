using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Core.Domain.Base.Result;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

/// <summary>
/// Command to create a product.
/// </summary>
public record CreateProductCommand(
    string Name,
    Guid CategoryId,
    decimal CostPrice,
    decimal SalePrice,
    decimal ItbisRate,
    int UnitType
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;
