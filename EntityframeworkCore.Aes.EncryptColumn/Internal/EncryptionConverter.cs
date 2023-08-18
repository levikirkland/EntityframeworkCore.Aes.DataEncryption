using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// 
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TProvider"></typeparam>
internal sealed class EncryptionConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    /// Creates a new <see cref="EncryptionConverter{TModel,TProvider}"/> instance.
    /// </summary>
    /// <param name="encryptionProvider">Encryption provider to use.</param>
    /// <param name="storageFormat">Encryption storage format.</param>
    /// <param name="mappingHints">Mapping hints.</param>
    public EncryptionConverter(IEncryptionProvider encryptionProvider, StorageFormat storageFormat, ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        string? inputData = input switch
        {
            string => input as string,
            _ => null,
        }; ; ;

        string encryptedRawBytes = encryptionProvider.Encrypt(inputData!);  //converting to string from byte[]

        if (encryptedRawBytes is null)
        {
            return default!;
        }

        object encryptedData = storageFormat switch
        {
            StorageFormat.Default => encryptedRawBytes,
            StorageFormat.Base64 => Convert.ToBase64String(Encoding.ASCII.GetBytes(encryptedRawBytes)),
            _ => encryptedRawBytes
        }; ;

        return (TOutput)Convert.ChangeType(encryptedData, typeof(TOutput));
    }

    private static TModel Decrypt<TInput, TOupout>(TProvider input, IEncryptionProvider encryptionProvider, StorageFormat storageFormat)
    {
        Type destinationType = typeof(TModel);
        string? inputData = storageFormat switch
        {
            StorageFormat.Default => input!.ToString()!,
            StorageFormat.Base64 => Encoding.UTF8.GetString(Convert.FromBase64String(input!.ToString()!)),
            _ => input as string
        };

        string decryptedRawBytes = encryptionProvider.Decrypt(inputData);
        object decryptedData = null;

        if (decryptedRawBytes != null && destinationType == typeof(string))
        {
            decryptedData = decryptedRawBytes;
        }


        return (TModel)Convert.ChangeType(decryptedData, typeof(TModel))!;
    }
}
