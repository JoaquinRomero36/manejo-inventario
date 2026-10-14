using System.Security.Cryptography;

namespace InventarioOperaciones.Security;

/// <summary>
/// Derivación de contraseñas con PBKDF2 (HMAC-SHA256) y sal por usuario.
/// Debe ser idéntico al usado por la App Admin para poder validar los mismos usuarios.
/// </summary>
public static class KeyDerivation
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);
        return Convert.ToBase64String(hash);
    }

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