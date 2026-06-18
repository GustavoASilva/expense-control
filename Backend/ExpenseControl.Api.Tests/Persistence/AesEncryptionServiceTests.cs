using ExpenseControl.Api.Persistence;

namespace ExpenseControl.Api.Tests.Persistence;

public class AesEncryptionServiceTests
{
    private readonly AesEncryptionService _sut = new("test-encryption-key");

    [Fact]
    public void Encrypt_ReturnsBase64String()
    {
        var ciphertext = _sut.Encrypt("hello world");

        Assert.True(IsBase64(ciphertext));
    }

    [Fact]
    public void Decrypt_ReturnsOriginalPlaintext()
    {
        var plaintext = "Monthly Salary";

        var ciphertext = _sut.Encrypt(plaintext);
        var decrypted = _sut.Decrypt(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachCall()
    {
        // Each encryption uses a random IV, so two calls with the same plaintext
        // must produce different ciphertexts
        var first = _sut.Encrypt("same input");
        var second = _sut.Encrypt("same input");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Decrypt_WithDifferentKey_ThrowsCryptographicException()
    {
        var ciphertext = _sut.Encrypt("sensitive data");

        var differentKey = new AesEncryptionService("wrong-key");

        Assert.Throws<System.Security.Cryptography.CryptographicException>(() =>
            differentKey.Decrypt(ciphertext));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("Monthly Rent 1500")]
    [InlineData("Grocery Shopping — week of April 7, 2026")]
    public void EncryptDecryptRoundTrip_VariousStrings(string plaintext)
    {
        var ciphertext = _sut.Encrypt(plaintext);
        var decrypted = _sut.Decrypt(ciphertext);

        Assert.Equal(plaintext, decrypted);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1500.00")]
    [InlineData("99999999999999999.99")]
    [InlineData("-250.50")]
    public void EncryptDecryptRoundTrip_DecimalStrings(string decimalString)
    {
        var ciphertext = _sut.Encrypt(decimalString);
        var decrypted = _sut.Decrypt(ciphertext);

        Assert.Equal(decimalString, decrypted);
    }

    [Fact]
    public void DifferentKeys_ProduceDifferentEncryption()
    {
        var service1 = new AesEncryptionService("key-one");
        var service2 = new AesEncryptionService("key-two");

        var ciphertext1 = service1.Encrypt("same plaintext");
        var ciphertext2 = service2.Encrypt("same plaintext");

        // Ciphertexts should differ (different keys → different output)
        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    private static bool IsBase64(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        var span = value.AsSpan().TrimEnd('=');
        foreach (var c in span)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '+' && c != '/')
                return false;
        }
        return value.Length % 4 == 0;
    }
}
