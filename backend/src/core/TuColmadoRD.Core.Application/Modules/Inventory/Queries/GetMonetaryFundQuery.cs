using MediatR;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Queries;

public record GetMonetaryFundQuery(Guid FundId)
    : IRequest<OperationResult<FundBalanceResponse, DomainError>>;
