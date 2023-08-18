using EntityframeworkCore.Aes.DataEncryption.Extensions;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Providers;

/// <summary>
/// Implements the Advanced Encryption Standard (AES) symmetric algorithm.
/// </summary>
public class AesProvider : IEncryptionProvider
{
    private readonly string _key;
    private const string prefix = "EncryptedValue:";

    public bool IsEncrypted(string text) =>
        text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    /// <summary>
    /// Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings.
    /// </summary>
    /// <param name="key">AES key used for the symmetric encryption.</param>

    public AesProvider(string key)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key), "");
    }

    /// <inheritdoc />
    public string Encrypt(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || IsEncrypted(input))
        {
            return input!;
        }
        var convertedKey = ConvertBase64ToByte(_key);

        if (convertedKey.Length == 0)
            throw new Exception("Bad or malformed key");

        byte[] vector = GenerateInitializationVector();

        string encryptedText = Convert.ToBase64String(EncryptString(input, convertedKey, vector));

        return prefix + Convert.ToBase64String(vector) + ";" + encryptedText;

    }

    private byte[] EncryptString(string plainText, byte[] key, byte[] vector)
    {
        using (var aes = Aes.Create())
        using (var encryptor = aes.CreateEncryptor(key, vector))
        using (var memoryStream = new MemoryStream())
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            using (var streamWriter = new StreamWriter(cryptoStream, Encoding.UTF8))
            {
                streamWriter.Write(plainText);
            }

            return memoryStream.ToArray();
        }
    }


    /// <inheritdoc />
    public string Decrypt(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !IsEncrypted(input))
        {
            // There is no need to decrypt null/empty or unencrypted text.
            return input;
        }

        var convertedKey = ConvertBase64ToByte(_key);

        if (convertedKey.Length == 0)
            throw new Exception("Bad or malformed key");

        byte[] vector = Convert.FromBase64String(input.Split(';')[0].Split(':')[1]);

        return DecryptString(Convert.FromBase64String(input.Split(';')[1]), convertedKey, vector);
    }

    private string DecryptString(byte[] encryptedBytes, byte[] key, byte[] vector)
    {
        using (var aes = Aes.Create())
        using (var decryptor = aes.CreateDecryptor(key, vector))
        using (var memoryStream = new MemoryStream(encryptedBytes))
        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
        using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
        {
            return streamReader.ReadToEnd();
        }
    }

    private byte[] GenerateInitializationVector()
    {
        var aes = Aes.Create();
        aes.GenerateIV();

        return aes.IV;
    }

    private byte[] ConvertBase64ToByte(string keyBase64)
    {
        if (keyBase64.IsBase64String())
        {
            return Convert.FromBase64String(keyBase64);
        }

        return new byte[0];
    }
}
