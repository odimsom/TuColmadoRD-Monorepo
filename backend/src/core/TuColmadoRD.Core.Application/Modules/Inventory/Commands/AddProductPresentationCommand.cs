using MediatR;
using TuColmadoRD.Core.Application.Behaviors;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Commands;

public record AddProductPresentationCommand(
    Guid ProductId,
    string DisplayName,
    int PresentationType,
    int SellMode,
    int MeasureUnit,
    decimal SalePrice,
    decimal CostPrice,
    string? Brand,
    decimal? NominalCapacity
) : IRequest<OperationResult<Guid, DomainError>>, ICommandMarker;
