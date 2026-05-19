using System.Text.Json;
using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Tenancy;
using TuColmadoRD.Core.Application.Inventory.Abstractions;
using TuColmadoRD.Core.Application.Inventory.Commands;
using TuColmadoRD.Core.Application.Inventory.DTOs;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Entities.Inventory;
using TuColmadoRD.Core.Domain.Entities.System;
using TuColmadoRD.Core.Domain.Enums.Inventory_Purchasing;
using ICategoryRepository = TuColmadoRD.Core.Domain.Interfaces.Repositories.Inventory.ICategoryRepository;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Inventory.Handlers;

/// <summary>
/// Handles product creation command.
/// </summary>
public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, OperationResult<Guid, DomainError>>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IProductRepository _productRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        ITenantProvider tenantProvider,
        IProductRepository productRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantProvider = tenantProvider;
        _productRepository = productRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<Guid, DomainError>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (Guid)_tenantProvider.TenantId;

        var taxResult = TaxRate.Create(request.ItbisRate);
        if (!taxResult.TryGetResult(out var itbisRate) || itbisRate is null)
        {
            return OperationResult<Guid, DomainError>.Bad(taxResult.Error);
        }

        var categoryExists = await _productRepository.CategoryExistsAsync(request.CategoryId, tenantId, cancellationToken);
        if (!categoryExists)
        {
            return OperationResult<Guid, DomainError>.Bad(DomainError.NotFound("category.not_found"));
        }

        var productResult = Product.Create(tenantId, request.Name, request.CategoryId, itbisRate!);
        if (!productResult.TryGetResult(out var product) || product is null)
        {
            return OperationResult<Guid, DomainError>.Bad(productResult.Error);
        }

        var payload = new ProductCreatedPayload(
            product.Id,
            tenantId,
            product.Name,
            product.CategoryId,
            product.ItbisRate.Rate,
            product.CreatedAt);

        var outboxMessage = new OutboxMessage("ProductCreated", JsonSerializer.Serialize(payload));

        await _productRepository.AddAsync(product, cancellationToken);
        await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return OperationResult<Guid, DomainError>.Good(product.Id);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Seeds the product catalogue with typical Dominican colmado items.
/// Creates one Product + one PackagedUnit presentation + one PackagedStock(qty=0) per entry.
/// Fails immediately if the tenant already has products.
/// </summary>
public sealed class SeedDefaultProductsCommandHandler
    : IRequestHandler<SeedDefaultProductsCommand, OperationResult<int, DomainError>>
{
    // ── Seed table ────────────────────────────────────────────────────────────
    private sealed record Def(
        string Name,
        string Category,
        string Display,
        decimal Sale,
        UnitOfMeasure Unit,
        decimal Tax = 0m);

    private static readonly Def[] Catalogue =
    [
        // ── Alimentos y Abarrotes — granos y víveres ───────────────────────
        new("Arroz",                     "Alimentos y Abarrotes", "Funda 1 lb",       8m,   UnitOfMeasure.Pound),
        new("Habichuelas rojas",         "Alimentos y Abarrotes", "Sobre 1 lb",      30m,   UnitOfMeasure.Pound),
        new("Habichuelas negras",        "Alimentos y Abarrotes", "Sobre 1 lb",      28m,   UnitOfMeasure.Pound),
        new("Espagueti Milano",          "Alimentos y Abarrotes", "Paquete 400g",    45m,   UnitOfMeasure.Package, 0.18m),
        new("Macarrón",                  "Alimentos y Abarrotes", "Paquete 400g",    40m,   UnitOfMeasure.Package, 0.18m),
        new("Harina de trigo",           "Alimentos y Abarrotes", "Sobre 1 lb",      20m,   UnitOfMeasure.Pound),
        new("Avena",                     "Alimentos y Abarrotes", "Sobre 1 lb",      35m,   UnitOfMeasure.Pound),
        new("Azúcar blanca",             "Alimentos y Abarrotes", "Funda 1 lb",      15m,   UnitOfMeasure.Pound),
        new("Sal",                       "Alimentos y Abarrotes", "Funda 1 lb",      10m,   UnitOfMeasure.Pound),
        new("Plátano verde",             "Alimentos y Abarrotes", "Unidad",          10m,   UnitOfMeasure.Unit),

        // ── Alimentos y Abarrotes — aceites y condimentos ──────────────────
        new("Aceite vegetal Mazola",     "Alimentos y Abarrotes", "Botella 1 litro", 180m,  UnitOfMeasure.Bottle, 0.18m),
        new("Aceite de oliva",           "Alimentos y Abarrotes", "Botella 1 litro", 350m,  UnitOfMeasure.Bottle, 0.18m),
        new("Salsa de tomate La Famosa", "Alimentos y Abarrotes", "Lata",            55m,   UnitOfMeasure.Unit,   0.18m),
        new("Pasta de tomate",           "Alimentos y Abarrotes", "Lata pequeña",    35m,   UnitOfMeasure.Unit,   0.18m),
        new("Sazón Goya",                "Alimentos y Abarrotes", "Sobrecito",       10m,   UnitOfMeasure.Package,0.18m),
        new("Knorr caldo de pollo",      "Alimentos y Abarrotes", "Cubito",           5m,   UnitOfMeasure.Unit,   0.18m),
        new("Orégano molido",            "Alimentos y Abarrotes", "Sobre",           10m,   UnitOfMeasure.Package,0.18m),
        new("Ají gustoso",               "Alimentos y Abarrotes", "Unidad",           5m,   UnitOfMeasure.Unit),
        new("Ajo",                       "Alimentos y Abarrotes", "Cabeza",          25m,   UnitOfMeasure.Unit),
        new("Vinagre blanco",            "Alimentos y Abarrotes", "Botella",         60m,   UnitOfMeasure.Bottle, 0.18m),

        // ── Alimentos y Abarrotes — enlatados ──────────────────────────────
        new("Sardinas Victorina",        "Alimentos y Abarrotes", "Lata",            65m,   UnitOfMeasure.Unit,   0.18m),
        new("Atún en agua",              "Alimentos y Abarrotes", "Lata",            80m,   UnitOfMeasure.Unit,   0.18m),
        new("Habichuelas guisadas Goya", "Alimentos y Abarrotes", "Lata",            75m,   UnitOfMeasure.Unit,   0.18m),
        new("Maíz en grano",             "Alimentos y Abarrotes", "Lata",            70m,   UnitOfMeasure.Unit,   0.18m),
        new("Pasta de guayaba",          "Alimentos y Abarrotes", "Lata",            55m,   UnitOfMeasure.Unit,   0.18m),

        // ── Lácteos y Huevos ───────────────────────────────────────────────
        new("Leche Rica",                "Lácteos y Huevos",      "Cartón 1 litro",  90m,   UnitOfMeasure.Liter),
        new("Leche en polvo Nestlé",     "Lácteos y Huevos",      "Sobre 200g",     120m,   UnitOfMeasure.Package,0.18m),
        new("Huevos",                    "Lácteos y Huevos",      "Unidad",          12m,   UnitOfMeasure.Unit),
        new("Mantequilla",               "Lácteos y Huevos",      "Barra",           75m,   UnitOfMeasure.Unit,   0.18m),
        new("Queso de hoja",             "Lácteos y Huevos",      "Porción 1 lb",   130m,   UnitOfMeasure.Pound,  0.18m),
        new("Yogur Yoplait",             "Lácteos y Huevos",      "Vaso",            45m,   UnitOfMeasure.Unit,   0.18m),

        // ── Carnes y Embutidos ─────────────────────────────────────────────
        new("Salami Induveca",           "Carnes y Embutidos",    "Porción 1 lb",   130m,   UnitOfMeasure.Pound,  0.18m),
        new("Jamón de pierna",           "Carnes y Embutidos",    "Porción 1 lb",   180m,   UnitOfMeasure.Pound,  0.18m),
        new("Longaniza",                 "Carnes y Embutidos",    "Porción 1 lb",   150m,   UnitOfMeasure.Pound,  0.18m),
        new("Chorizo",                   "Carnes y Embutidos",    "Porción 1 lb",   160m,   UnitOfMeasure.Pound,  0.18m),
        new("Pollo entero",              "Carnes y Embutidos",    "Por libra",       75m,   UnitOfMeasure.Pound),
        new("Chuleta de cerdo",          "Carnes y Embutidos",    "Por libra",      120m,   UnitOfMeasure.Pound),

        // ── Bebidas (no alcohólicas) ───────────────────────────────────────
        new("Agua Planeta Azul",         "Bebidas",               "Botella 500ml",   30m,   UnitOfMeasure.Bottle, 0.18m),
        new("Coca-Cola",                 "Bebidas",               "Lata 355ml",      65m,   UnitOfMeasure.Unit,   0.18m),
        new("Pepsi",                     "Bebidas",               "Lata 355ml",      60m,   UnitOfMeasure.Unit,   0.18m),
        new("Jugo Rica",                 "Bebidas",               "Cartón 200ml",    25m,   UnitOfMeasure.Unit,   0.18m),
        new("Jugo de chinola",           "Bebidas",               "Botella",         95m,   UnitOfMeasure.Bottle, 0.18m),
        new("Café Santo Domingo molido", "Bebidas",               "Sobre 50g",       45m,   UnitOfMeasure.Package,0.18m),
        new("Café Induban",              "Bebidas",               "Sobre 50g",       40m,   UnitOfMeasure.Package,0.18m),
        new("Chocolate Cortes",          "Bebidas",               "Barra",           55m,   UnitOfMeasure.Unit,   0.18m),

        // ── Bebidas alcohólicas (en categoría Bebidas) ─────────────────────
        new("Cerveza Presidente",        "Bebidas",               "Botella 330ml",   90m,   UnitOfMeasure.Bottle, 0.18m),
        new("Cerveza Bohemia",           "Bebidas",               "Botella 330ml",   85m,   UnitOfMeasure.Bottle, 0.18m),
        new("Presidente Light",          "Bebidas",               "Botella 330ml",   90m,   UnitOfMeasure.Bottle, 0.18m),
        new("Ron Brugal Añejo",          "Bebidas",               "Botella 750ml",  650m,   UnitOfMeasure.Bottle, 0.18m),
        new("Ron Barceló Añejo",         "Bebidas",               "Botella 750ml",  700m,   UnitOfMeasure.Bottle, 0.18m),

        // ── Snacks y Dulces ────────────────────────────────────────────────
        new("Papitas Lay's",             "Snacks y Dulces",       "Bolsa pequeña",   25m,   UnitOfMeasure.Package,0.18m),
        new("Cheetos",                   "Snacks y Dulces",       "Bolsa pequeña",   25m,   UnitOfMeasure.Package,0.18m),
        new("Mantecaditos",              "Snacks y Dulces",       "Paquete",         35m,   UnitOfMeasure.Package,0.18m),
        new("Galletas Oreo",             "Snacks y Dulces",       "Paquete",         55m,   UnitOfMeasure.Package,0.18m),
        new("Galletas Ritz",             "Snacks y Dulces",       "Paquete",         65m,   UnitOfMeasure.Package,0.18m),
        new("Chicharrón",                "Snacks y Dulces",       "Bolsa",           40m,   UnitOfMeasure.Package,0.18m),
        new("Chiclets Adams",            "Snacks y Dulces",       "Paquete",         15m,   UnitOfMeasure.Package,0.18m),
        new("Caramelos",                 "Snacks y Dulces",       "Unidad",           3m,   UnitOfMeasure.Unit,   0.18m),
        new("Paletas Heladas",           "Snacks y Dulces",       "Unidad",          25m,   UnitOfMeasure.Unit,   0.18m),
        new("Chocolatín Kinder",         "Snacks y Dulces",       "Unidad",          45m,   UnitOfMeasure.Unit,   0.18m),

        // ── Higiene Personal ───────────────────────────────────────────────
        new("Jabón de baño Palmolive",   "Higiene Personal",      "Barra",           55m,   UnitOfMeasure.Unit,   0.18m),
        new("Champú Head & Shoulders",   "Higiene Personal",      "Sobre",           20m,   UnitOfMeasure.Package,0.18m),
        new("Pasta dental Colgate",      "Higiene Personal",      "Tubo pequeño",    85m,   UnitOfMeasure.Unit,   0.18m),
        new("Papel higiénico Scott",     "Higiene Personal",      "Rollo",           30m,   UnitOfMeasure.Unit,   0.18m),
        new("Desodorante Speed Stick",   "Higiene Personal",      "Stick",          180m,   UnitOfMeasure.Unit,   0.18m),
        new("Toallas sanitarias Always", "Higiene Personal",      "Paquete",        120m,   UnitOfMeasure.Package,0.18m),

        // ── Limpieza y Hogar ───────────────────────────────────────────────
        new("Detergente Fab",            "Limpieza y Hogar",      "Sobre 250g",      35m,   UnitOfMeasure.Package,0.18m),
        new("Cloro Clorox",              "Limpieza y Hogar",      "Botella 500ml",   55m,   UnitOfMeasure.Bottle, 0.18m),
        new("Suavizante Suavitel",       "Limpieza y Hogar",      "Sobre",           15m,   UnitOfMeasure.Package,0.18m),
        new("Jabón de lavar Rinso",      "Limpieza y Hogar",      "Barra",           40m,   UnitOfMeasure.Unit,   0.18m),
        new("Escoba",                    "Limpieza y Hogar",      "Unidad",         150m,   UnitOfMeasure.Unit,   0.18m),
        new("Fabuloso",                  "Limpieza y Hogar",      "Botella 500ml",   80m,   UnitOfMeasure.Bottle, 0.18m),

        // ── Otros — tabaco ─────────────────────────────────────────────────
        new("Cigarrillos Marlboro",      "Otros",                 "Cajetilla",      180m,   UnitOfMeasure.Package,0.18m),
        new("Cigarrillos Winston",       "Otros",                 "Cajetilla",      160m,   UnitOfMeasure.Package,0.18m),
        new("Cigarrillo suelto",         "Otros",                 "Unidad",          15m,   UnitOfMeasure.Unit,   0.18m),
    ];

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly ITenantProvider        _tenant;
    private readonly ICategoryRepository    _categories;
    private readonly IProductRepository     _products;
    private readonly IPresentationRepository _presentations;
    private readonly IPackagedStockRepository _stocks;
    private readonly IUnitOfWork            _uow;

    public SeedDefaultProductsCommandHandler(
        ITenantProvider tenantProvider,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IPresentationRepository presentationRepository,
        IPackagedStockRepository packagedStockRepository,
        IUnitOfWork unitOfWork)
    {
        _tenant        = tenantProvider;
        _categories    = categoryRepository;
        _products      = productRepository;
        _presentations = presentationRepository;
        _stocks        = packagedStockRepository;
        _uow           = unitOfWork;
    }

    public async Task<OperationResult<int, DomainError>> Handle(
        SeedDefaultProductsCommand request, CancellationToken ct)
    {
        var tenantId = (Guid)_tenant.TenantId;

        if (await _products.AnyAsync(tenantId, ct))
            return OperationResult<int, DomainError>.Bad(
                DomainError.Business("products.already_seeded",
                    "Ya existen productos para este colmado. El catálogo no fue modificado."));

        // Build category name → id lookup
        var existing = await _categories.FindAsync(
            c => c.TenantId.Value == tenantId && c.IsActive, cancellationToken: ct);
        var catMap = existing.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

        var zeroTax = TaxRate.Create(0m).Result!;
        var itbis18 = TaxRate.Create(0.18m).Result!;

        int created = 0;

        foreach (var d in Catalogue)
        {
            if (!catMap.TryGetValue(d.Category, out var catId)) continue;

            var rate = d.Tax == 0m ? zeroTax : itbis18;

            if (!Product.Create(tenantId, d.Name, catId, rate).TryGetResult(out var product) || product is null)
                continue;

            var cost = Money.FromDecimal(Math.Round(d.Sale * 0.80m, 2)).Result!;
            var sale = Money.FromDecimal(d.Sale).Result!;

            if (!ProductPresentation.Create(
                    tenantId, product.Id, d.Display,
                    PresentationType.PackagedUnit, SellMode.ByUnit,
                    d.Unit, sale, cost, null, null)
                .TryGetResult(out var pres) || pres is null)
                continue;

            if (!PackagedStock.Create(tenantId, pres.Id).TryGetResult(out var stock) || stock is null)
                continue;

            await _products.AddAsync(product, ct);
            await _presentations.AddAsync(pres, ct);
            await _stocks.AddAsync(stock, ct);
            created++;
        }

        if (created > 0)
            await _uow.CommitAsync(ct);

        return OperationResult<int, DomainError>.Good(created);
    }
}
