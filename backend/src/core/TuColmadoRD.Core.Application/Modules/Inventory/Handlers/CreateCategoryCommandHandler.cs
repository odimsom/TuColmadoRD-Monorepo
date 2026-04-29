using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using ResultUnit = TuColmadoRD.Core.Domain.Base.Result.Unit;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ITenantProvider tenantProvider, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var result = Category.Create(tenantId, request.Name);
        if (!result.TryGetResult(out var category) || category is null)
            return OperationResult<Guid, DomainError>.Bad(result.Error);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(category.Id);
    }
}

public sealed class SeedDefaultCategoriesCommandHandler : IRequestHandler<SeedDefaultCategoriesCommand, OperationResult<int, DomainError>>
{
    private static readonly string[] DefaultNames =
    [
        "Alimentos y Abarrotes",
        "Bebidas",
        "Lácteos y Huevos",
        "Carnes y Embutidos",
        "Limpieza y Hogar",
        "Higiene Personal",
        "Snacks y Dulces",
        "Congelados",
        "Otros",
    ];

    private readonly ITenantProvider _tenantProvider;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SeedDefaultCategoriesCommandHandler(ITenantProvider tenantProvider, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<int, DomainError>> Handle(SeedDefaultCategoriesCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var existing = await _categoryRepository.FindAsync(
            c => c.TenantId.Value == tenantId && c.IsActive,
            cancellationToken: cancellationToken);

        var existingNames = existing.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        int created = 0;

        foreach (var name in DefaultNames)
        {
            if (existingNames.Contains(name)) continue;

            var result = Category.Create(tenantId, name);
            if (!result.TryGetResult(out var category) || category is null) continue;

            await _categoryRepository.AddAsync(category, cancellationToken);
            created++;
        }

        if (created > 0)
            await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<int, DomainError>.Good(created);
    }
}

public sealed class DeactivateCategoryCommandHandler : IRequestHandler<DeactivateCategoryCommand, OperationResult<ResultUnit, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCategoryCommandHandler(ITenantProvider tenantProvider, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<ResultUnit, DomainError>> Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var matches = await _categoryRepository.FindAsync(
            c => c.Id == request.CategoryId && c.TenantId.Value == tenantId,
            cancellationToken: cancellationToken);
        var category = matches.FirstOrDefault();

        if (category is null)
            return OperationResult<ResultUnit, DomainError>.Bad(DomainError.NotFound("category.not_found", "Categoría no encontrada."));

        category.Deactivate();
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<ResultUnit, DomainError>.Good(ResultUnit.Value);
    }
}
