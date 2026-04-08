using System.Security.Cryptography;
using System.Text;

namespace ExpenseControl.Api.Persistence;

/// <summary>
/// Encrypts and decrypts field values using AES-256-CBC with a random IV per encryption.
/// A 256-bit key is derived from the configured key string using SHA-256.
/// The encrypted output is Base64-encoded with the 16-byte IV prepended to the ciphertext.
/// </summary>
public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(string key)
    {
        // Derive a stable 256-bit key from the provided passphrase
        _key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }

    /// <inheritdoc/>
    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Prepend the 16-byte IV so each ciphertext is self-contained
        var result = new byte[aes.IV.Length + ciphertext.Length];
        aes.IV.CopyTo(result, 0);
        ciphertext.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertext)
    {
        var data = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = _key;

        // The first 16 bytes are the IV
        aes.IV = data[..16];
        var encrypted = data[16..];

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
