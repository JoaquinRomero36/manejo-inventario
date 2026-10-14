using System.Security.Cryptography;

namespace InventarioAdmin.Security;

/// <summary>
/// Derivación de contraseñas con PBKDF2 (HMAC-SHA256) y sal por usuario.
/// Nunca se guarda la contraseña en claro ni se registra en logs.
/// </summary>
public static class KeyDerivation
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    /// <summary>Genera una sal aleatoria criptográficamente segura (Base64).</summary>
    public static string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(salt);
    }

    /// <summary>Deriva la contraseña usando la sal indicada y devuelve el hash en Base64.</summary>
    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);
        return Convert.ToBase64String(hash);
    }

    /// <summary>Compara en tiempo constante el hash esperado con el calculado.</summary>
    public static bool VerifyPassword(string password, string salt, string expectedHash)
    {
        try
        {
            var computed = HashPassword(password, salt);
            var a = Convert.FromBase64String(computed);
            var b = Convert.FromBase64String(expectedHash);
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}