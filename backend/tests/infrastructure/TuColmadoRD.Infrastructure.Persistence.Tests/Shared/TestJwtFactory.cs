using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace TuColmadoRD.Tests.Shared;

public static class TestJwtFactory
{
    public static (string publicKeyPem, string privateKeyPem) GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);
        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        return (publicKey, privateKey);
    }

    public static string GenerateToken(string privateKeyPem, Guid tenantId, Guid terminalId, DateTime validUntil)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        var creds = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        var validUntilUtc = DateTime.SpecifyKind(validUntil, DateTimeKind.Utc);

        var claims = new List<Claim>
        {
            new("tenant_id", tenantId.ToString()),
            new("terminal_id", terminalId.ToString()),
            new("valid_until", new DateTimeOffset(validUntilUtc).ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: creds,
            expires: validUntilUtc);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
