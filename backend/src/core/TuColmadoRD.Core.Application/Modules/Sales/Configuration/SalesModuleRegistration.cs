using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TuColmadoRD.Core.Application.Sales.Abstractions;
using TuColmadoRD.Core.Application.Sales.Commands;
using TuColmadoRD.Core.Application.Sales.Handlers;
using TuColmadoRD.Core.Application.Sales.Queries;
using TuColmadoRD.Core.Application.Sales.Validators;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Sales.Configuration;

public static class SalesModuleRegistration
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<CreateSaleCommand, OperationResult<CreateSaleResult, DomainError>>, CreateSaleCommandHandler>();
        services.AddScoped<IRequestHandler<VoidSaleCommand, OperationResult<ResultUnit, DomainError>>, VoidSaleCommandHandler>();

        services.AddScoped<ISaleService, SaleService>();

        services.AddScoped<IValidator<CreateSaleCommand>, CreateSaleCommandValidator>();
        services.AddScoped<IValidator<VoidSaleCommand>, VoidSaleCommandValidator>();

        return services;
    }
}
