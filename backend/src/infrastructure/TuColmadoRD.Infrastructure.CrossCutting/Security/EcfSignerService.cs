using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Xml;
using TuColmadoRD.Core.Application.Interfaces.Services;

namespace TuColmadoRD.Infrastructure.CrossCutting.Security;

public class EcfSignerService : IEcfSignerService
{
    // For test purposes, we'll use an ephemeral self-signed certificate if one isn't found.
    // In production, this would fetch from TenantProfile or a secure vault.

    public Task<string> SignXmlAsync(string xmlContent, Guid tenantId)
    {
        try
        {
            var doc = new XmlDocument
            {
                PreserveWhitespace = true
            };
            doc.LoadXml(xmlContent);

            // Fetch real cert based on tenantId here. 
            // Mocking for testing
            using var cert = GetOrGenerateTestCertificate(tenantId);
            var rsaKey = cert.GetRSAPrivateKey() ?? throw new InvalidOperationException("Private key is missing or not RSA.");

            var signedXml = new SignedXml(doc)
            {
                SigningKey = rsaKey
            };

            var reference = new Reference
            {
                Uri = "" // Sign the whole document
            };

            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(cert));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();
            doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            return Task.FromResult(doc.OuterXml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to sign e-CF XML.", ex);
        }
    }

    private X509Certificate2 GetOrGenerateTestCertificate(Guid tenantId)
    {
        // Simple self-signed for dev/testing
        var ecdsa = RSA.Create(2048);
        var req = new CertificateRequest($"CN=Test_Tenant_{tenantId}", ecdsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return req.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
    }
}
