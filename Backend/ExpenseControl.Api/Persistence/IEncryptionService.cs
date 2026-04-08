namespace ExpenseControl.Api.Persistence;

/// <summary>
/// Provides application-level encryption and decryption for sensitive database fields.
/// Implementations must be deterministic in decryption but may use random IVs in encryption.
/// </summary>
public interface IEncryptionService
{
    /// <summary>Encrypts a plaintext string and returns a Base64-encoded ciphertext.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a Base64-encoded ciphertext and returns the original plaintext.</summary>
    string Decrypt(string ciphertext);
}
