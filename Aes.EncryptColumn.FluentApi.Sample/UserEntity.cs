namespace Aes.EncryptColumn.FluentApi.Sample
{
    public class UserEntity
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Notes { get; set; }

        public string EncryptedData { get; set; }

        public string EncryptedDataAsString { get; set; }
    }
}
