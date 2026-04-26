using System;
using System.Threading.Tasks;

namespace TuColmadoRD.Core.Application.Interfaces.Services;

public interface IEcfSignerService
{
    /// <summary>
    /// Signs an e-CF XML using the tenant's digital certificate.
    /// </summary>
    /// <param name="xmlContent">The raw XML string to sign.</param>
    /// <param name="tenantId">The ID of the tenant to load their certificate.</param>
    /// <returns>The XML string with the XMLDSig applied.</returns>
    Task<string> SignXmlAsync(string xmlContent, Guid tenantId);
}
