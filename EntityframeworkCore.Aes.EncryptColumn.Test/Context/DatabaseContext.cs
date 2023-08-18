using Microsoft.EntityFrameworkCore.DataEncryption;

namespace Microsoft.EntityFrameworkCore.Aes.EncryptColumn.Test.Context;

public class DatabaseContext : DbContext
{
    private readonly IEncryptionProvider _encryptionProvider;

    public DbSet<AuthorEntity> Authors { get; set; }

    public DbSet<BookEntity> Books { get; set; }

    public DatabaseContext(DbContextOptions options)
        : base(options)
    { }

    public DatabaseContext(DbContextOptions options, IEncryptionProvider encryptionProvider = null)
        : base(options)
    {
        _encryptionProvider = encryptionProvider;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseEncryption(_encryptionProvider);
    }
}
