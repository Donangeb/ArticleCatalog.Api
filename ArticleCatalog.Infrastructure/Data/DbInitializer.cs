using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArticleCatalog.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task ApplyMigrationsAsync(ArticleCatalogDbContext dbContext, ILogger logger)
        {
            try
            {
                // Открываем соединение
                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                // Проверяем, существует ли таблица миграций
                using var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '__EFMigrationsHistory');";

                var result = await command.ExecuteScalarAsync();
                bool migrationsTableExists = result is bool b && b;

                if (!migrationsTableExists)
                {
                    logger.LogWarning("Migrations history table not found. Calling EnsureCreated...");
                    await dbContext.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database schema created via EnsureCreated.");
                    return;
                }

                // Если таблица миграций существует — ищем неприменённые миграции
                var pending = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Any())
                {
                    logger.LogInformation("Pending migrations: {Migrations}", string.Join(", ", pending));
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied.");
                }
                else
                {
                    logger.LogInformation("Database is up-to-date.");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Migration check failed. Fallback to EnsureCreated.");
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database schema created using EnsureCreated fallback.");
            }
        }

    }
}
