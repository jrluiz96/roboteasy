using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Api.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class Argon2PasswordHasher : IPasswordHasher
{
    private readonly Argon2Settings _settings;

    public Argon2PasswordHasher(IOptions<Argon2Settings> settings)
    {
        _settings = settings.Value;
    }

    public string Hash(string password)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        
        // Formato: $argon2id$v=19$m={memory},t={iterations},p={parallelism}${salt}${hash}
        return $"$argon2id$v=19$m={_settings.MemorySize},t={_settings.Iterations},p={_settings.Parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 6 || parts[1] != "argon2id")
                return false;

            // Parse params
            var paramParts = parts[3].Split(',');
            var memory = int.Parse(paramParts[0].Replace("m=", ""));
            var iterations = int.Parse(paramParts[1].Replace("t=", ""));
            var parallelism = int.Parse(paramParts[2].Replace("p=", ""));

            var salt = Convert.FromBase64String(parts[4]);
            var hash = Convert.FromBase64String(parts[5]);

            var computedHash = HashPassword(password, salt, memory, iterations, parallelism);
            
            return CryptographicOperations.FixedTimeEquals(hash, computedHash);
        }
        catch
        {
            return false;
        }
    }

    private byte[] HashPassword(string password, byte[] salt, int? memory = null, int? iterations = null, int? parallelism = null)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt;
        argon2.MemorySize = memory ?? _settings.MemorySize;
        argon2.Iterations = iterations ?? _settings.Iterations;
        argon2.DegreeOfParallelism = parallelism ?? _settings.Parallelism;

        return argon2.GetBytes(_settings.HashLength);
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[_settings.SaltLength];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}

public class Argon2Settings
{
    /// <summary>
    /// Memória em KB (padrão: 65536 = 64MB)
    /// </summary>
    public int MemorySize { get; set; } = 65536;
    
    /// <summary>
    /// Número de iterações (padrão: 4)
    /// </summary>
    public int Iterations { get; set; } = 4;
    
    /// <summary>
    /// Paralelismo - número de threads (padrão: 2)
    /// </summary>
    public int Parallelism { get; set; } = 2;
    
    /// <summary>
    /// Tamanho do hash em bytes (padrão: 32)
    /// </summary>
    public int HashLength { get; set; } = 32;
    
    /// <summary>
    /// Tamanho do salt em bytes (padrão: 16)
    /// </summary>
    public int SaltLength { get; set; } = 16;
}
