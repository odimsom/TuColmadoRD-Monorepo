# Sales Module

Complete point-of-sale module implementing clean architecture with domain-driven design, CQRS patterns, and event-driven architecture.

## Quick Start

### Creating a Sale

```csharp
// 1. Prepare items
var items = new List<CreateSaleItemDto>
{
    new(productId1, quantity: 5, unitPrice: 100),
    new(productId2, quantity: 3, unitPrice: 50)
};

// 2. Send command through MediatR
var command = new CreateSaleCommand(
    items,
    customerName: "Juan Pérez",
    paymentMethods: new[] {
        new PaymentMethodDto(1, "cash", 1234.5m, null)
    }
);

var result = await mediator.Send(command);
if (result.IsGood)
{
    var sale = result.Value;
    Console.WriteLine($"Sale {sale.ReceiptNumber} created");
}
```

### Voiding a Sale

```csharp
var command = new VoidSaleCommand(
    saleId: Guid.Parse("..."),
    voidReason: "Customer change of mind");

var result = await mediator.Send(command);
if (result.IsGood)
    Console.WriteLine("Sale voided successfully");
```

### Querying Sales

```csharp
var service = serviceProvider.GetRequiredService<ISaleService>();

// Get single sale
var saleResult = await service.GetSaleDetailAsync(saleId, tenantId, ct);

// Get paged sales
var paginationResult = await service.GetPagedSalesAsync(
    tenantId, pageNumber: 1, pageSize: 20, ct);

foreach (var sale in paginationResult.Value.Items)
{
    Console.WriteLine($"{sale.ReceiptNumber}: {sale.Total}");
}
```

## Module Structure

```
Sales/
├── Commands/              # Write operations
│   ├── CreateSaleCommand
│   └── VoidSaleCommand
├── Queries/               # Read operations
│   └── SaleService
├── Abstractions/          # Interfaces and DTOs
│   ├── ISaleService
│   ├── ICommandMarker
│   ├── PaymentMethodDto
│   └── ...
├── DTOs/                  # Data transfer objects
│   └── SaleDtos.cs
├── Validators/            # Input validation
│   └── *Validators.cs
├── Outbox/                # Event payloads
│   ├── SaleCreatedPayload
│   └── SaleVoidedPayload
└── Handlers/              # Future: handler registration
```

## Key Features

### ✅ Inventory Management
- Auto-deduct stock on sale creation
- Auto-restore stock on sale void
- Prevents overselling

### ✅ Financial Reconciliation
- Automatic ITBIS calculation
- Payment method tracking
- Change calculation
- Shift total integration

### ✅ Shift Integration
- Requires open shift to create sales
- Validates sale ownership
- Updates shift running totals
- Prevents orphaned sales

### ✅ Event-Driven
- Outbox pattern for reliability
- `SaleCreated` events on new sales
- `SaleVoided` events on void
- Ready for async event processors

### ✅ Multi-Tenancy
- All queries tenant-filtered
- Terminal isolation
- Prevents cross-tenant data leaks

### ✅ Atomic Operations
- All related changes in single transaction
- Sale, inventory, shift updates together
- Outbox events persisted atomically

## Domain Model

### Sale Aggregate
```
Sale (Aggregate Root)
├── Id (Guid) - Primary Key
├── ShiftId (Guid) - FK to Shift
├── TerminalId (Guid) - Terminal context
├── ReceiptNumber (string) - Unique receipt ID
├── CashierName (string)
├── Status (SaleStatus) - pending|completed|voided
├── Items (SaleItem[]) - Sale line items
├── Payments (Payment[]) - Payment breakdown
├── Totals (Money) - Subtotal, ITBIS, Total
├── CreatedAt (DateTime)
├── VoidedAt (DateTime?) - Null if not voided
└── VoidReason (string?) - Why voided

Invariants:
- Total = Subtotal + ITBIS
- Total ≥ TotalPaid (can overpay)
- Status transitions: pending → completed → voided
- Cannot be modified after voiding
- Items must not be empty
```

## Commands

### CreateSaleCommand
Creates a new sale with inventory deduction and shift update.

**Flow:**
1. Validate command (items not empty, discount valid)
2. Verify open shift exists
3. Load products and validate stock
4. Create Sale aggregate
5. Deduct inventory
6. Update shift totals
7. Create `SaleCreated` outbox event
8. Atomically persist

**Errors:**
- `shift.not_found` - No open shift
- `product.not_found` - Product doesn't exist
- `product.insufficient_stock` - Not enough inventory

### VoidSaleCommand
Reverses a completed sale with full reconciliation.

**Flow:**
1. Validate command
2. Verify open shift exists
3. Load sale and validate
4. Ensure sale belongs to current shift
5. Restore inventory
6. Mark as voided
7. Reverse shift totals
8. Create `SaleVoided` outbox event
9. Atomically persist

**Errors:**
- `sale.not_found` - Sale doesn't exist
- `sale.wrong_shift` - Sale from different shift

## Queries

### ISaleService Interface

```csharp
// Get single sale with all details
Task<OperationResult<Sale, DomainError>> GetSaleDetailAsync(
    Guid saleId, Guid tenantId, CancellationToken ct);

// Get all sales from a terminal
Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByTerminalAsync(
    Guid terminalId, Guid tenantId, CancellationToken ct);

// Get all sales from a shift
Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByShiftAsync(
    Guid shiftId, Guid tenantId, CancellationToken ct);

// Get paginated sales with filtering
Task<OperationResult<SalePaginationResult, DomainError>> GetPagedSalesAsync(
    Guid tenantId, int pageNumber, int pageSize, CancellationToken ct);
```

## Error Handling

All operations return `OperationResult<T, DomainError>`:

```csharp
var result = await mediator.Send(command);

if (result.IsGood)
{
    // Success - access result.Value
    var sale = result.Value;
}
else
{
    // Failure - check result.Error
    var error = result.Error;
    Console.WriteLine($"Error: {error.Code} - {error.Message}");
}
```

### Common Error Codes

| Code | Meaning | Recoverable |
|------|---------|------------|
| `shift.not_found` | No open shift exists | Yes - open shift and retry |
| `product.not_found` | Product not found | Yes - use valid product |
| `product.insufficient_stock` | Not enough inventory | Yes - reduce quantity |
| `sale.not_found` | Sale doesn't exist | No - check sale ID |
| `sale.wrong_shift` | Sale from different shift | No - void from correct shift |

## Testing

### Unit Tests

Run with:
```bash
dotnet test TuColmadoRD.Core.Application.Tests
```

Test file: `tests/core/TuColmadoRD.Core.Application.Tests/Sales/SalesModuleTests.cs`

**Coverage:**
- ✅ Create sale success path
- ✅ Create sale without open shift
- ✅ Void sale success path
- ✅ Void sale from wrong shift
- ✅ Mock repository verification

### Fixtures

Helper classes for test data:
- `SaleFixture` - Create test sales
- `ProductFixture` - Create test products
- `ShiftFixture` - Create test shifts

## Configuration

### Register with Dependency Injection

```csharp
// In ServiceRegistration.cs
services.AddScoped<ISaleService, SaleService>();
services.AddScoped<ISaleRepository, SaleRepository>(); // Implement repo
services.AddMediatR(typeof(CreateSaleCommand).Assembly);
```

### MediatR Pipeline

Commands automatically run through:
1. Validation middleware (FluentValidation)
2. Logging middleware
3. Transaction middleware (if needed)
4. Command handler

## Database Schema

### Sales Table
```sql
CREATE TABLE Sales (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ShiftId UNIQUEIDENTIFIER NOT NULL,
    TerminalId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ReceiptNumber NVARCHAR(50) NOT NULL UNIQUE,
    CashierName NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    SubtotalAmount DECIMAL(18,2) NOT NULL,
    TotalItbis DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL,
    VoidedAt DATETIME2,
    VoidReason NVARCHAR(200),
    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id),
    FOREIGN KEY (TerminalId) REFERENCES Terminals(Id),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);
```

### SaleItems Table
```sql
CREATE TABLE SaleItems (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    SaleId UNIQUEIDENTIFIER NOT NULL,
    ProductId UNIQUEIDENTIFIER NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    ItbisRate DECIMAL(5,2) NOT NULL,
    LineSubtotal DECIMAL(18,2) NOT NULL,
    LineItbis DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (SaleId) REFERENCES Sales(Id) ON DELETE CASCADE
);
```

## API Endpoints

(Pending implementation)

```http
POST /api/sales
GET /api/sales/{id}
GET /api/sales?page=1&pageSize=20
POST /api/sales/{id}/void
```

## Integration Points

### Shift Module
- Verifies open shift before creating sale
- Updates shift running totals
- Reverses totals on void

### Product Module
- Validates product existence
- Checks inventory levels
- Deducts stock on create
- Restores stock on void

### Payment Module
- Accepts payment methods
- Validates total paid
- Calculates change

### Outbox/Messaging
- Publishes SaleCreated event
- Publishes SaleVoided event
- Ensures reliable event delivery

## Performance Considerations

- **Pagination**: Always use paged queries for large result sets
- **Indexes**: Add indexes on ShiftId, TerminalId, CreatedAt for queries
- **Batch Operations**: Consider batch void for administrator cleanup
- **Caching**: Cache products and shift data for repeated queries

## Best Practices

1. **Always validate shift**: Don't assume shift is open
2. **Handle errors gracefully**: Check `OperationResult.IsGood`
3. **Use pagination**: Avoid loading entire sales history
4. **Implement idempotency**: Handle duplicate create commands
5. **Log operations**: Track all creates and voids for audit
6. **Test edge cases**: Decimal precision, large quantities

## Implementation Status

- ✅ Domain model and aggregates
- ✅ Command handlers (create, void)
- ✅ Query service
- ✅ Input validation
- ✅ Unit tests
- ✅ Architecture documentation
- ⏳ EF Core repositories
- ⏳ API controllers
- ⏳ Integration tests
- ⏳ Outbox processor
- ⏳ API documentation

## Related Documentation

- [Full Architecture Guide](./ARCHITECTURE.md)
- Shift Module Documentation
- Product Module Documentation
- Payment Module Documentation

---

**Module Status**: Core implementation complete, awaiting EF Core repositories and API integration  
**Last Updated**: 2024  
**Maintainers**: Sales Team
