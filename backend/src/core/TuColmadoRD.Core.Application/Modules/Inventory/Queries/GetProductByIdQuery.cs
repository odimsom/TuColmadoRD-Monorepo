using MediatR;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

/// <summary>
/// Query to retrieve a product by id.
/// </summary>
public record GetProductByIdQuery(Guid ProductId)
    : IRequest<OperationResult<ProductDto, DomainError>>;
