namespace EntityframeworkCore.Aes.DataEncryption.Extensions
{
    public static class StringExtensions
    {
        public static bool IsBase64String(this string str)
        {
            Span<byte> buffer = new Span<byte>(new byte[str.Length]);
            return Convert.TryFromBase64String(str, buffer, out int bytesParsed);
        }
    }
}
