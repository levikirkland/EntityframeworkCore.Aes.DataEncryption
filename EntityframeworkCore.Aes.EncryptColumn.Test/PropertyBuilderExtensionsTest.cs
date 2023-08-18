using Bogus;
using Microsoft.EntityFrameworkCore.Aes.EncryptColumn.Test.Context;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Encryption.Test;

public class PropertyBuilderExtensionsTest
{
    private static readonly Faker _faker = new();

    [Fact]
    public void PropertyBuilderShouldNeverBeNullTest()
    {
        Assert.Throws<ArgumentNullException>(() => PropertyBuilderExtensions.IsEncrypted<object>(null));
    }

    [Fact]
    public void PropertyShouldHaveEncryptionAnnotationsTest()
    {
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var provider = new AesProvider(key);

        string name = _faker.Name.FullName();
        string strings = _faker.Random.ToString()!;

        UserEntity user = new()
        {
            Name = name,
            NameAsBytes = name,
            ExtraData = strings,
            ExtraDataAsBytes = strings,
            EmptyString = null
        };

        using var contextFactory = new DatabaseContextFactory();
        using (var context = contextFactory.CreateContext<FluentDbContext>(provider))
        {
            Assert.NotNull(context);

            IEntityType entityType = context.GetUserEntityType();
            Assert.NotNull(entityType);

            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.Name)), true, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.NameAsBytes)), true, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.ExtraData)), true, StorageFormat.Base64);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.ExtraDataAsBytes)), true, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.Id)), false, StorageFormat.Default);
            AssertPropertyAnnotations(entityType.GetProperty(nameof(UserEntity.EmptyString)), true, StorageFormat.Base64);

            context.Users.Add(user);
            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext<FluentDbContext>(provider))
        {
            UserEntity u = context.Users.First();

            Assert.NotNull(u);
            Assert.Equal(name, u.Name);
            Assert.Equal(name, u.NameAsBytes);
            Assert.Equal(strings, u.ExtraData);
            Assert.Equal(strings, u.ExtraDataAsBytes);
            Assert.Null(u.EmptyString);
        }
    }

    private static void AssertPropertyAnnotations(IProperty property, bool shouldBeEncrypted, StorageFormat expectedStorageFormat)
    {
        Assert.NotNull(property);

        IAnnotation encryptedAnnotation = property.FindAnnotation(PropertyAnnotations.IsEncrypted)!;

        if (shouldBeEncrypted)
        {
            Assert.NotNull(encryptedAnnotation);
            Assert.True((bool)encryptedAnnotation.Value);

            IAnnotation formatAnnotation = property.FindAnnotation(PropertyAnnotations.StorageFormat)!;
            Assert.NotNull(formatAnnotation);
            Assert.Equal(expectedStorageFormat, formatAnnotation.Value);
        }
        else
        {
            Assert.Null(encryptedAnnotation);
            Assert.Null(property.FindAnnotation(PropertyAnnotations.StorageFormat));
        }
    }

    private class UserEntity
    {
        public int Id { get; set; }

        // Encrypted as default (Base64)
        public string Name { get; set; }

        // Encrypted as raw byte array.
        public string NameAsBytes { get; set; }

        // Encrypted as Base64 string
        public string ExtraData { get; set; }

        // Encrypted as raw byte array.
        public string ExtraDataAsBytes { get; set; }

        // Encrypt as Base64 string, but will be empty.
        public string EmptyString { get; set; }
    }

    private class FluentDbContext : DbContext
    {
        private readonly IEncryptionProvider _encryptionProvider;

        public DbSet<UserEntity> Users { get; set; }

        public FluentDbContext(DbContextOptions options)
        : base(options)
        { }

        public FluentDbContext(DbContextOptions options, IEncryptionProvider encryptionProvider)
            : base(options)
        {
            _encryptionProvider = encryptionProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userEntityBuilder = modelBuilder.Entity<UserEntity>();

            userEntityBuilder.HasKey(x => x.Id);
            userEntityBuilder.Property(x => x.Id).IsRequired().ValueGeneratedOnAdd();
            userEntityBuilder.Property(x => x.Name).IsRequired().IsEncrypted();
            userEntityBuilder.Property(x => x.NameAsBytes).IsRequired().HasColumnType("BLOB").IsEncrypted(StorageFormat.Default);
            userEntityBuilder.Property(x => x.ExtraData).IsRequired().HasColumnType("TEXT").IsEncrypted(StorageFormat.Base64);
            userEntityBuilder.Property(x => x.ExtraDataAsBytes).IsRequired().HasColumnType("BLOB").IsEncrypted(StorageFormat.Default);
            userEntityBuilder.Property(x => x.EmptyString).IsRequired(false).HasColumnType("TEXT").IsEncrypted(StorageFormat.Base64);

            modelBuilder.UseEncryption(_encryptionProvider);
        }

        public IEntityType GetUserEntityType() => Model.FindEntityType(typeof(UserEntity))!;
    }
}
