using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microwave.Domain.Interfaces;

namespace Microwave.Infrastructure.Services;

/// <summary>
/// Serviço de criptografia responsável por hashing de senhas SHA1, criptografia AES e geração/validação de tokens JWT
/// </summary>
public class CryptographyService : ICryptographyService
{
    private readonly string _jwtSecret;
    private readonly string _encryptionKey;

    public CryptographyService(IConfiguration configuration)
    {
        _jwtSecret = configuration["Jwt:Secret"] ?? "MicrowaveWaveEngineCore2025SecretKey123!@#$%^&*()";
        _encryptionKey = configuration["Encryption:Key"] ?? "WaveEngine2025!@#";
    }

    /// <summary>
    /// Gera hash SHA1 da senha combinada com chave de encriptação para segurança adicional
    /// </summary>
    /// <param name="password">Senha em texto plano</param>
    /// <returns>Hash SHA1 em Base64</returns>
    public string HashPassword(string password)
    {
        using var sha1 = SHA1.Create();
        var hashedBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password + _encryptionKey));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    public string EncryptConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = GetKeyBytes(_encryptionKey);
            aes.IV = new byte[16];

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainTextBytes = Encoding.UTF8.GetBytes(connectionString);
            var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch
        {
            return connectionString;
        }
    }

    public string DecryptConnectionString(string encryptedConnectionString)
    {
        if (string.IsNullOrEmpty(encryptedConnectionString))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = GetKeyBytes(_encryptionKey);
            aes.IV = new byte[16];

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var encryptedBytes = Convert.FromBase64String(encryptedConnectionString);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            return encryptedConnectionString;
        }
    }

    public string GenerateJwtToken(string username, TimeSpan expiration)
    {
        if (expiration <= TimeSpan.Zero)
            expiration = TimeSpan.FromMinutes(1);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, username),
                new Claim("username", username)
            ]),
            Expires = DateTime.UtcNow.Add(expiration),
            NotBefore = DateTime.UtcNow,
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    public bool ValidateJwtToken(string token, out string? username)
    {
        username = null;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            username = jwtToken.Claims.First(x => x.Type == "username").Value;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private byte[] GetKeyBytes(string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        Array.Resize(ref keyBytes, 32);
        return keyBytes;
    }
}
