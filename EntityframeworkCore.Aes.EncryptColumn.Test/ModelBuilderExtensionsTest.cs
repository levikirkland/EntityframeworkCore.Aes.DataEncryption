using Microsoft.EntityFrameworkCore.Aes.EncryptColumn.Test.Context;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test;

public class ModelBuilderExtensionsTest
{
    [Fact]
    public void ModelBuilderShouldNeverBeNullTest()
    {
        Assert.Throws<ArgumentNullException>(() => ModelBuilderExtensions.UseEncryption(null, null));
    }

    [Fact]
    public void EncryptionProviderShouldNeverBeNullTest()
    {
        using var contextFactory = new DatabaseContextFactory();

        Assert.Throws<ArgumentNullException>(() => contextFactory.CreateContext<InvalidPropertyDbContext>(null));
    }

    [Fact]
    public void UseEncryptionWithUnsupportedTypeTest()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var provider = new AesProvider(key);
        using var contextFactory = new DatabaseContextFactory();

        Assert.Throws<NotImplementedException>(() => contextFactory.CreateContext<InvalidPropertyDbContext>(provider));
    }

    private class InvalidPropertyEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Encrypted]
        public string Name { get; set; }

        [Encrypted]
        public int Age { get; set; }
    }

    private class InvalidPropertyDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<InvalidPropertyEntity> InvalidEntities { get; set; }

        public InvalidPropertyDbContext(DbContextOptions options)
        : base(options)
        { }

        public InvalidPropertyDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(_encryptionProvider);
        }
    }
}
