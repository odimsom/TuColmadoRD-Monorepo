namespace TuColmadoRD.Core.Application.DTOs.Sync;

public record ProductSyncDto(
    Guid ProductId, 
    string Name, 
    decimal Price, 
    Guid CategoryId, 
    bool IsActive, 
    DateTime UpdatedAt
);
