using Chroma.Application.Abstractions;
using Chroma.Domain.Entities;
using Chroma.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Chroma.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static readonly (string Key, string Description)[] DefaultPermissions =
    [
        ("contacts.read", "View contacts"),
        ("contacts.create", "Create contacts"),
        ("contacts.update", "Update contacts"),
        ("contacts.delete", "Delete contacts"),
        ("companies.read", "View companies"),
        ("companies.create", "Create companies"),
        ("companies.update", "Update companies"),
        ("companies.delete", "Delete companies"),
        ("users.read", "View users"),
        ("users.create", "Create users"),
        ("users.update", "Update users"),
        ("roles.read", "View roles"),
        ("roles.create", "Create roles"),
        ("roles.update", "Update roles"),
        ("roles.manage_permissions", "Manage role permissions"),
        ("tenant.settings.read", "View tenant settings"),
        ("tenant.settings.update", "Update tenant settings"),
        ("tags.read", "View tags"),
        ("tags.create", "Create tags"),
        ("tags.update", "Update tags"),
        ("tags.delete", "Delete tags"),
        ("pipelines.read", "View pipelines"),
        ("pipelines.create", "Create pipelines"),
        ("pipelines.update", "Update pipelines"),
        ("pipelines.delete", "Delete pipelines"),
        ("deals.read", "View deals"),
        ("deals.create", "Create deals"),
        ("deals.update", "Update deals"),
        ("deals.delete", "Delete deals"),
        ("deals.move_stage", "Move deal stage"),
        ("notes.read", "View notes"),
        ("notes.create", "Create notes"),
        ("notes.update", "Update notes"),
        ("notes.delete", "Delete notes"),
        ("tasks.read", "View tasks"),
        ("tasks.create", "Create tasks"),
        ("tasks.update", "Update tasks"),
        ("tasks.delete", "Delete tasks"),
        ("activities.read", "View activities"),
        ("activities.create", "Create activities"),
        ("activities.update", "Update activities"),
        ("activities.delete", "Delete activities"),
        ("channels.read", "View channels"),
        ("channels.create", "Create channels"),
        ("channels.update", "Update channels"),
        ("channels.delete", "Delete channels"),
        ("conversations.read", "View conversations"),
        ("conversations.update", "Update conversations"),
        ("messages.read", "View messages"),
        ("messages.send", "Send messages"),
        ("forms.read", "View forms"),
        ("forms.create", "Create forms"),
        ("forms.update", "Update forms"),
        ("forms.delete", "Delete forms"),
        ("custom_fields.read", "View custom fields"),
        ("custom_fields.create", "Create custom fields"),
        ("custom_fields.update", "Update custom fields"),
        ("custom_fields.delete", "Delete custom fields"),
        ("files.read", "View files"),
        ("files.create", "Upload files"),
        ("files.delete", "Delete files"),
        ("workflows.read", "View workflows"),
        ("workflows.create", "Create workflows"),
        ("workflows.update", "Update workflows"),
        ("workflows.delete", "Delete workflows"),
        ("notifications.read", "View notifications"),
        ("notifications.update", "Update notifications"),
        ("reports.read", "View reports")
    ];

    public static async Task SeedAsync(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<SeedOptions> seedOptions,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var options = seedOptions.Value;

        foreach (var (key, description) in DefaultPermissions)
        {
            var exists = await dbContext.Permissions.AnyAsync(x => x.Key == key, cancellationToken);
            if (!exists)
            {
                dbContext.Permissions.Add(new Permission { Key = key, Description = description });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(x => x.Slug == options.TenantSlug, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = options.TenantName,
                Slug = options.TenantSlug,
                Email = options.AdminEmail,
                Status = "active"
            };
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded tenant {TenantSlug}", options.TenantSlug);
        }

        var settings = await dbContext.TenantSettings.FirstOrDefaultAsync(x => x.TenantId == tenant.Id, cancellationToken);
        if (settings is null)
        {
            dbContext.TenantSettings.Add(new TenantSettings
            {
                TenantId = tenant.Id,
                Theme = "light",
                Language = "tr",
                Currency = "TRY",
                TimeZone = "UTC"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.TenantId == tenant.Id && x.Name == "Admin",
            cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role { TenantId = tenant.Id, Name = "Admin" };
            dbContext.Roles.Add(adminRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var allPermissions = await dbContext.Permissions.AsNoTracking().ToListAsync(cancellationToken);
        var existingRolePermissions = await dbContext.RolePermissions
            .Where(x => x.RoleId == adminRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permission in allPermissions.Where(p => !existingRolePermissions.Contains(p.Id)))
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var adminEmail = options.AdminEmail.Trim().ToLowerInvariant();
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(
            x => x.TenantId == tenant.Id && x.Email == adminEmail,
            cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
                TenantId = tenant.Id,
                FirstName = options.AdminFirstName,
                LastName = options.AdminLastName,
                Email = adminEmail,
                PasswordHash = passwordHasher.Hash(options.AdminPassword),
                Status = "active"
            };
            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded admin user {AdminEmail}", adminEmail);
        }

        var hasAdminRole = await dbContext.UserRoles.AnyAsync(
            x => x.UserId == adminUser.Id && x.RoleId == adminRole.Id,
            cancellationToken);

        if (!hasAdminRole)
        {
            dbContext.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await SeedDemoPipelineAsync(dbContext, tenant.Id, cancellationToken);
    }

    private static async Task SeedDemoPipelineAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var pipelineExists = await dbContext.Pipelines.AnyAsync(x => x.TenantId == tenantId, cancellationToken);
        if (pipelineExists)
        {
            return;
        }

        var pipeline = new Pipeline { TenantId = tenantId, Name = "Sales Pipeline", Order = 1 };
        dbContext.Pipelines.Add(pipeline);
        await dbContext.SaveChangesAsync(cancellationToken);

        var stages = new[]
        {
            new Stage { TenantId = tenantId, PipelineId = pipeline.Id, Name = "Lead", Order = 1, Color = "#94a3b8" },
            new Stage { TenantId = tenantId, PipelineId = pipeline.Id, Name = "Qualified", Order = 2, Color = "#60a5fa" },
            new Stage { TenantId = tenantId, PipelineId = pipeline.Id, Name = "Proposal", Order = 3, Color = "#a78bfa" },
            new Stage { TenantId = tenantId, PipelineId = pipeline.Id, Name = "Won", Order = 4, Color = "#22c55e", IsWinStage = true },
            new Stage { TenantId = tenantId, PipelineId = pipeline.Id, Name = "Lost", Order = 5, Color = "#ef4444", IsLostStage = true }
        };

        dbContext.Stages.AddRange(stages);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
