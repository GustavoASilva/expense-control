using System.Security.Cryptography;
using System.Text;

namespace ExpenseControl.Api.Persistence;

/// <summary>
/// Encrypts and decrypts field values using AES-256-CBC with a random IV per encryption.
/// The 256-bit encryption key is derived from the configured passphrase using PBKDF2-SHA256
/// with a fixed application-specific salt and 100,000 iterations.
/// The encrypted output is Base64-encoded with the 16-byte IV prepended to the ciphertext.
/// </summary>
public sealed class AesEncryptionService : IEncryptionService
{
    // Fixed application-specific salt — not secret, prevents cross-application key reuse
    private static readonly byte[] ApplicationSalt = Encoding.UTF8.GetBytes("ExpenseControl.Api.FieldEncryption.v1");

    private readonly byte[] _key;

    public AesEncryptionService(string key)
    {
        // Derive a 256-bit key from the passphrase using PBKDF2-SHA256
        _key = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(key),
            salt: ApplicationSalt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);
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
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Ciphertext cannot be null or empty.", nameof(ciphertext));

        byte[] data;
        try
        {
            data = Convert.FromBase64String(ciphertext);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("The stored value is not valid Base64. The field may not be encrypted or the data is corrupt.", ex);
        }

        const int ivSize = 16; // AES block size in bytes
        if (data.Length < ivSize)
            throw new CryptographicException("The stored value is too short to contain a valid AES IV. The field may not be encrypted or the data is corrupt.");

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = data[..ivSize];
        var encrypted = data[ivSize..];

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
