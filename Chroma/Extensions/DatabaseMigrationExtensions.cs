using Chroma.Application.Abstractions;
using Chroma.Infrastructure.Options;
using Chroma.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Chroma.Extensions;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            logger.LogInformation("Applying pending EF Core migrations...");
            await dbContext.Database.MigrateAsync();

            // Güvenlik ağı: AccentColor kolonu migration kaçırıldıysa ekle
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE tenant_settings
                ADD COLUMN IF NOT EXISTS "AccentColor" character varying(40) NOT NULL DEFAULT 'violet';
                """);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE files
                ADD COLUMN IF NOT EXISTS "Category" character varying(40) NOT NULL DEFAULT 'document';
                ALTER TABLE files
                ADD COLUMN IF NOT EXISTS "StorageKey" character varying(1000) NOT NULL DEFAULT '';
                ALTER TABLE files
                ADD COLUMN IF NOT EXISTS "UploadedByUserId" uuid NULL;
                UPDATE files
                SET "StorageKey" = TRIM(BOTH '/' FROM "Url")
                WHERE ("StorageKey" IS NULL OR "StorageKey" = '')
                  AND "Url" IS NOT NULL
                  AND "Url" <> '';
                """);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE appointments
                ADD COLUMN IF NOT EXISTS "SessionType" character varying(40) NOT NULL DEFAULT 'follow_up';
                ALTER TABLE appointments
                ADD COLUMN IF NOT EXISTS "SessionSummary" text NULL;
                ALTER TABLE appointments
                ADD COLUMN IF NOT EXISTS "PrivateNotes" text NULL;
                ALTER TABLE appointments
                ADD COLUMN IF NOT EXISTS "NextSteps" text NULL;
                ALTER TABLE appointments
                ADD COLUMN IF NOT EXISTS "ProgressScore" integer NULL;
                """);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE conversations
                ADD COLUMN IF NOT EXISTS "Title" character varying(200) NULL;
                ALTER TABLE conversations
                ADD COLUMN IF NOT EXISTS "IsGroup" boolean NOT NULL DEFAULT FALSE;
                ALTER TABLE messages
                ADD COLUMN IF NOT EXISTS "FileId" uuid NULL;
                """);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE conversation_participants
                ADD COLUMN IF NOT EXISTS "UnreadCount" integer NOT NULL DEFAULT 0;
                """);

            logger.LogInformation("Database migrations applied successfully.");

            await DatabaseSeeder.SeedAsync(
                dbContext,
                scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
                scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>(),
                logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations or seed data.");
            throw;
        }
    }
}
