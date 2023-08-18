using Aes.EncryptColumn.FluentApi.Sample;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;

internal class Program
{
    static void Main(string[] args)
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connection)
            .Options;

        // AES key randomly generated at each run.
        var aes = System.Security.Cryptography.Aes.Create();
        aes.GenerateKey();
        var key = Convert.ToBase64String(aes.Key);
        var encryptionProvider = new AesProvider(key);

        using (var context = new DatabaseContext(options, encryptionProvider))
        {
            context.Database.EnsureCreated();

            var user = new UserEntity
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@doe.com",
                Notes = "Hello world!",
                EncryptedData = "Does this work at all",
                EncryptedDataAsString = "this is a test"
            };

            context.Users.Add(user);
            context.SaveChanges();

            Console.WriteLine($"Users count: {context.Users.Count()}");
        }

        using (var context = new EncryptedDatabaseContext(options))
        {
            UserEntity user = context.Users.First();

            Console.WriteLine($"Encrypted User: {user.FirstName} {user.LastName} - {user.Email} (Notes: {user.Notes})");
        }

        using (var context = new DatabaseContext(options, encryptionProvider))
        {
            UserEntity user = context.Users.First();

            Console.WriteLine($"User: {user.FirstName} {user.LastName} - {user.Email} (Notes: {user.Notes})");
        }
    }
}
