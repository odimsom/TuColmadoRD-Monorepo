using TuColmadoRD.Core.Domain.Base;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Domain.Entities.System;

/// <summary>
/// Stores the fiscal and business profile for a tenant (colmado).
/// Required for DGI-compliant receipts: business name, RNC, and address
/// must appear on every NCF-stamped invoice (Norma 06-18 DGII).
/// </summary>
public class TenantProfile : ITenantEntity
{
    private TenantProfile()
    {
        TenantId = null!;
        BusinessName = string.Empty;
        BusinessAddress = string.Empty;
    }

    public Guid Id { get; private set; }
    public TenantIdentifier TenantId { get; private set; }

    /// <summary>Business name as registered with DGII.</summary>
    public string BusinessName { get; private set; }

    /// <summary>RNC del negocio (9 dígitos, verificado).</summary>
    public Rnc? Rnc { get; private set; }

    /// <summary>Physical address for fiscal receipts.</summary>
    public string BusinessAddress { get; private set; }

    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static OperationResult<TenantProfile, DomainError> Create(
        TenantIdentifier tenantId,
        string businessName,
        string businessAddress,
        string? phone = null,
        string? email = null,
        Rnc? rnc = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            return OperationResult<TenantProfile, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessName_required"));

        if (businessName.Length > 200)
            return OperationResult<TenantProfile, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessName_too_long"));

        if (string.IsNullOrWhiteSpace(businessAddress))
            return OperationResult<TenantProfile, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessAddress_required"));

        if (businessAddress.Length > 500)
            return OperationResult<TenantProfile, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessAddress_too_long"));

        var now = DateTime.UtcNow;
        return OperationResult<TenantProfile, DomainError>.Good(new TenantProfile
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BusinessName = businessName.Trim(),
            BusinessAddress = businessAddress.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim(),
            Rnc = rnc,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public OperationResult<Unit, DomainError> Update(
        string businessName,
        string businessAddress,
        string? phone,
        string? email,
        Rnc? rnc)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessName_required"));

        if (string.IsNullOrWhiteSpace(businessAddress))
            return OperationResult<Unit, DomainError>.Bad(
                DomainError.Validation("tenantProfile.businessAddress_required"));

        BusinessName = businessName.Trim();
        BusinessAddress = businessAddress.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Rnc = rnc;
        UpdatedAt = DateTime.UtcNow;

        return OperationResult<Unit, DomainError>.Good(Unit.Value);
    }
}
