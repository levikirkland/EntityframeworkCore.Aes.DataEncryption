using Microsoft.EntityFrameworkCore;

namespace Aes.EncryptColumn.FluentApi.Sample
{
    public class EncryptedDatabaseContext : DatabaseContext
    {
        public EncryptedDatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options, null)
        {
        }
    }
}
