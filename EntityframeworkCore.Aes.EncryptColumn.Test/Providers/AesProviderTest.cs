using Bogus;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Aes.EncryptColumn.Test.Providers;

public class AesProviderTest
{
    private static readonly Faker _faker = new();

    [Fact]
    public void CreateAesProviderWithoutKeyTest()
    {
        Assert.Throws<ArgumentNullException>(() => new AesProvider(null));
    }

    [Fact]
    public void EncryptNullOrEmptyDataTest()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var provider = new AesProvider(key);

        Assert.NotNull(provider.Encrypt(null));
        Assert.Null(provider.Encrypt(string.Empty));
    }

    [Fact]
    public void DecryptNullOrEmptyDataTest()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var provider = new AesProvider(key);

        Assert.Null(provider.Decrypt(null));
        Assert.NotNull(provider.Decrypt(string.Empty));
    }

    [Fact]
    public void EncryptDecryptByteArrayTest()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var provider = new AesProvider(key);
        var input = "New Brown Cow";

        string encryptedData = provider.Encrypt(input);
        Assert.NotNull(encryptedData);

        string decryptedData = provider.Decrypt(encryptedData);
        Assert.NotNull(decryptedData);

        Assert.Equal(input, decryptedData);
    }
}
