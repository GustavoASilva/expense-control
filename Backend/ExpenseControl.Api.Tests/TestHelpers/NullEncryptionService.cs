using ExpenseControl.Api.Persistence;

namespace ExpenseControl.Api.Tests.TestHelpers;

/// <summary>
/// No-op encryption service used in unit tests.
/// Values pass through unchanged so the in-memory database stores readable values
/// and arithmetic aggregations (Sum, GroupBy) continue to work without SQL translation issues.
/// </summary>
public sealed class NullEncryptionService : IEncryptionService
{
    public string Encrypt(string plaintext) => plaintext;
    public string Decrypt(string ciphertext) => ciphertext;
}
