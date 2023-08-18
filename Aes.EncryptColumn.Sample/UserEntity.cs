using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aes.EncryptColumn.Sample
{
    public class UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Encrypted]
        public string Email { get; set; }

        [Required]
        [Encrypted(StorageFormat.Base64)]
        public string Notes { get; set; }

        [Required]
        [Encrypted]
        public string EncryptedData { get; set; }

        [Required]
        [Encrypted(StorageFormat.Base64)]
        [Column(TypeName = "TEXT")]
        public string EncryptedDataAsString { get; set; }
    }
}
