# Sales Module - Complete Architecture Documentation

## Overview
The Sales Module implements a clean architecture for managing point-of-sale operations with inventory management, financial tracking, and event-driven components.

## Module Structure

### Core Components

#### 1. **Command Layer** (`Sales/Commands/`)
Commands represent write operations and state changes.

**CreateSaleCommand** (`CreateSaleCommand.cs`)
- Validates and processes new sale creation
- **Process Flow:**
  1. Verify open shift exists for terminal
  2. Load products and validate stock availability
  3. Calculate ITBIS and apply discounts
  4. Create Sale aggregate with items and payments
  5. Deduct stock from products
  6. Update shift totals
  7. Create outbox event for SaleCreated
  8. Atomically persist all changes

- **Key Validations:**
  - Sale items must not be empty
  - Discount cannot exceed subtotal
  - Only active products allowed
  - Sufficient stock must exist

**VoidSaleCommand** (`VoidSaleCommand.cs`)
- Reverses completed sales with full reconciliation
- **Process Flow:**
  1. Verify open shift exists
  2. Load and validate sale
  3. Ensure sale belongs to current shift
  4. Restore product stock (reverse deductions)
  5. Mark sale as voided with reason
  6. Reverse shift totals
  7. Create outbox event for SaleVoided
  8. Atomically persist all changes

- **Constraints:**
  - Only fully completed sales can be voided
  - Must void from same shift created
  - Cannot be re-voided

#### 2. **Query Layer** (`Sales/Queries/`)

**SaleService** (`SaleService.cs`)
- Read-only operations for retrieving sales data
- **Operations:**
  - `GetSalesByTerminalAsync()` - All sales from a terminal
  - `GetSalesByShiftAsync()` - All sales from a shift
  - `GetSaleDetailAsync()` - Complete sale with items & payments
  - `GetPagedSalesAsync()` - Paginated listing with filtering

- **Key Features:**
  - No state mutations
  - Pagination support
  - Filtering by terminal/shift
  - Efficient data retrieval

#### 3. **Domain Layer** (`Entities/`)

**Sale Aggregate Root**
```
Sale
├── SaleId (PK)
├── ShiftId (FK)
├── TerminalId
├── ReceiptNumber (unique)
├── CashierName
├── Status (enum: pending, completed, voided)
├── Items (SaleItem[])
│   ├── ProductId
│   ├── Quantity
│   ├── UnitPrice
│   ├── ItbisRate
│   └── LineTotal
├── Payments (Payment[])
│   ├── PaymentMethodId
│   ├── Amount
│   └── Reference (optional)
├── Totals
│   ├── Subtotal
│   ├── TotalItbis
│   ├── Total
│   └── TotalPaid
├── Notes (optional)
├── CreatedAt
├── VoidedAt (nullable)
└── VoidReason (nullable)
```

**Invariants:**
- Total must equal Subtotal + ITBIS
- Total paid must match sum of payments
- Items list must not be empty
- Status transitions: pending → completed → voided (terminal)
- Once voided, cannot be modified

#### 4. **DTOs Layer** (`Sales/DTOs/`)

All request/response DTOs for API contracts:
- `SaleDetailDto` - Complete sale information
- `SaleSummaryDto` - Lightweight listing summary
- `ReceiptDto` - Print-ready format
- `PagedResult<T>` - Pagination wrapper

#### 5. **Abstractions & Interfaces** (`Sales/Abstractions/`)

**ISaleRepository**
```csharp
Task AddAsync(Sale sale, CancellationToken ct);
Task UpdateAsync(Sale sale, CancellationToken ct);
Task<Sale?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct);
Task<IEnumerable<Sale>?> GetByShiftIdAsync(Guid shiftId, Guid tenantId, CancellationToken ct);
Task<IEnumerable<Sale>?> GetByTerminalIdAsync(Guid terminalId, Guid tenantId, CancellationToken ct);
Task<(IEnumerable<Sale>, int)> GetPagedAsync(Guid tenantId, int pageNum, int pageSize, CancellationToken ct);
```

**ICurrentShiftService**
```csharp
Task<OperationResult<Shift, DomainError>> GetOpenShiftOrFailAsync(
    Guid tenantId, Guid terminalId, CancellationToken ct);
```

**ISaleService** (Query operations)
```csharp
Task<OperationResult<Sale, DomainError>> GetSaleDetailAsync(...);
Task<OperationResult<IEnumerable<Sale>, DomainError>> GetSalesByTerminalAsync(...);
Task<OperationResult<SalePaginationResult, DomainError>> GetPagedSalesAsync(...);
```

**IOutboxRepository** (Event publishing)
```csharp
Task AddAsync(OutboxMessage message, CancellationToken ct);
```

#### 6. **Validation** (`Sales/Validators/`)

Formula validators ensure domain rules:
- `CreateSaleCommandValidator` - Input validation
- `VoidSaleCommandValidator` - Void request validation
- FluentValidation integration for command handlers

#### 7. **Events** (`Sales/Outbox/`)

**SaleCreatedPayload**
```csharp
{
    SaleId, TenantId, TerminalId,
    ReceiptNumber, ItemCount,
    TotalAmount, CreatedAt
}
```

**SaleVoidedPayload**
```csharp
{
    SaleId, TenantId, TerminalId,
    ReceiptNumber, VoidReason, VoidedAt
}
```

Events are persisted in Outbox table for:
- Asynchronous processing
- Saga orchestration
- Reporting data sync
- Financial reconciliation

## Data Flow Diagrams

### Create Sale Flow
```
CreateSaleCommand
    ↓
Validate command structure
    ↓
Get open shift or fail
    ↓
Load products by IDs
    ↓
Check stock + calculate totals
    ↓
Create Sale aggregate with domain logic
    ↓
Deduct stock from products
    ↓
Update shift totals
    ↓
Create SaleCreated outbox event
    ↓
Atomically persist (Sale, Products, Shift, OutboxMessage)
    ↓
Return OperationResult<Sale>
```

### Void Sale Flow
```
VoidSaleCommand
    ↓
Validate command structure
    ↓
Get open shift or fail
    ↓
Load sale by ID or fail
    ↓
Verify sale.ShiftId == currentShiftId
    ↓
Load products for all sale items
    ↓
Restore stock (add back quantities)
    ↓
Mark sale as voided with reason
    ↓
Reverse shift totals
    ↓
Create SaleVoided outbox event
    ↓
Atomically persist (Sale, Products, Shift, OutboxMessage)
    ↓
Return OperationResult<Unit>
```

### Query Sales Flow
```
GetPagedSalesAsync(tenantId, pageNum, pageSize)
    ↓
Validate pagination params
    ↓
Query sales with offset/limit
    ↓
Get total count
    ↓
Return SalePaginationResult
```

## Integration Points

### With Shift Module
- Validates open shift before sale operations
- Updates shift totals (cash/card summary)
- Prevents orphaned sales from closed shifts

### With Product Module
- Validates product existence and availability
- Deducts inventory on sale creation
- Restores inventory on sale void
- Prevents overselling

### With Payment Module
- Accepts payment methods and amounts
- Validates total paid matches sale total
- Calculates change due

### With Tenant Module
- Enforces tenant isolation (all queries filtered)
- Ensures multi-tenancy compliance
- Prevents cross-tenant data leaks

### With Outbox Pattern
- Persists events in OutboxMessage table
- Enables asynchronous processing
- Supports eventual consistency
- Facilitates reporting and analytics

## Error Handling Strategy

All operations return `OperationResult<T, DomainError>` with:
- **Business Errors** (handled, expected)
  - `sale.already_exists` - Duplicate receipt number
  - `sale.not_found` - Sale doesn't exist
  - `sale.wrong_shift` - Sale from different shift
  - `shift.not_found` - No open shift
  - `product.insufficient_stock` - Not enough inventory
  - `product.not_found` - Product doesn't exist
  
- **Validation Errors** (input validation)
  - Empty items list
  - Negative quantities
  - Invalid discount
  
- **System Errors** (unexpected)
  - Database failures
  - Transaction rollback
  - Concurrent modification

## Testing Strategy

### Unit Tests (`SalesModuleTests.cs`)

**Test Scenarios:**
1. **Create Sale Success** - Valid items create sale and deduct stock
2. **Create Sale No Shift** - Returns error if no open shift
3. **Create Sale Insufficient Stock** - Returns error if stock unavailable
4. **Void Sale Success** - Valid void reverses stock and updates shift
5. **Void Sale Wrong Shift** - Returns error if sale from different shift
6. **Void Sale Not Found** - Returns error if sale doesn't exist

**Testing Approach:**
- Mock all repositories and services
- Assert both return values and side effects (repository calls)
- Verify atomic persistence (all changes or none)
- Test isolation and multi-tenancy

### Integration Tests (Strategy)
- Real database with transactions
- Full command-query roundtrip
- Verify outbox events created
- Test concurrent operations
- Validate shift-sale consistency

## API Endpoint Examples

```http
POST /api/sales
Content-Type: application/json

{
  "items": [
    {"productId": "...", "quantity": 5, "unitPrice": 100},
    {"productId": "...", "quantity": 3, "unitPrice": 50}
  ],
  "customerName": "Juan Pérez",
  "paymentMethods": [
    {"methodId": 1, "amount": 650}
  ]
}

Response: 201 Created
{
  "saleId": "...",
  "receiptNumber": "RCP-2024-001",
  "status": "completed",
  "total": 1234.5
}
```

```http
POST /api/sales/{saleId}/void
Content-Type: application/json

{"voidReason": "Customer request - defective item"}

Response: 200 OK
{
  "success": true,
  "message": "Venta anulada exitosamente"
}
```

```http
GET /api/sales?page=1&pageSize=20

Response: 200 OK
{
  "items": [...],
  "page": 1,
  "pageSize": 20,
  "totalCount": 150,
  "totalPages": 8
}
```

## Implementation Checklist

- ✅ Domain entities (Sale, SaleItem, Payment)
- ✅ Value objects (Money, Receipt Numbers)
- ✅ CreateSaleCommand handler with validation
- ✅ VoidSaleCommand handler with reconciliation
- ✅ SaleService for read operations
- ✅ Repository interfaces
- ✅ DTOs for API contracts
- ✅ Outbox event payloads
- ✅ Unit test suite
- ⏳ Repository implementations (EF Core)
- ⏳ API controllers and endpoints
- ⏳ Input validators (FluentValidation)
- ⏳ Integration tests
- ⏳ Event handlers (background service)
- ⏳ API documentation (OpenAPI/Swagger)

## Key Design Decisions

1. **Command-Query Segregation**: Separate handlers for writes (commands) and reads (queries) for scalability
2. **Atomic Transactions**: All related changes (sale, stock, shift, outbox) in single transaction
3. **Event Sourcing Ready**: Outbox pattern enables eventual consistency and prevents data loss
4. **Domain-Driven**: Business logic in aggregates, not in handlers
5. **Multi-Tenancy First**: All queries filtered by tenant ID
6. **Fail-Fast Validation**: Validate input before any state changes
7. **Reversible Operations**: Void provides full reconciliation, not just logical deletion

## Dependencies

- **MediatR**: Command/query bus
- **FluentValidation**: Input validation
- **EF Core**: Data persistence
- **Serilog**: Logging
- **xUnit**: Testing framework
- **Moq**: Test mocking
- **FluentAssertions**: Test assertions

---

**Module Owner**: Sales Team  
**Last Updated**: 2024  
**Status**: Core implementation complete, awaiting EF Core repositories
