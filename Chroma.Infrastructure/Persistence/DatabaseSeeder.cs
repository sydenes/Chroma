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
        ("conversations.create", "Create conversations"),
        ("conversations.update", "Update conversations"),
        ("conversations.assign", "Assign conversations"),
        ("conversations.update_status", "Update conversation status"),
        ("conversations.mark_read", "Mark conversations read"),
        ("conversations.delete", "Delete conversations"),
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
        ("reports.read", "View reports"),
        ("appointments.read", "View appointments"),
        ("appointments.create", "Create appointments"),
        ("appointments.update", "Update appointments"),
        ("appointments.delete", "Delete appointments"),
        ("offers.read", "View offer packages"),
        ("offers.create", "Create offer packages"),
        ("offers.update", "Update offer packages"),
        ("offers.delete", "Delete offer packages")
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

        var allPermissions = await dbContext.Permissions.AsNoTracking().ToListAsync(cancellationToken);

        var adminEmail = options.AdminEmail.Trim().ToLowerInvariant();
        var adminUser = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == adminEmail, cancellationToken);

        if (adminUser is null)
        {
            adminUser = new User
            {
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

        var customers = new[]
        {
            new CustomerSeed(
                "Aristo Psikolojik Danışmanlık",
                "aristo-psikolojik-danismanlik",
                "Danışan Süreci",
                [
                    ("İlk Başvuru", "#94a3b8", false, false),
                    ("Ön Görüşme", "#60a5fa", false, false),
                    ("Seans Planlandı", "#a78bfa", false, false),
                    ("Aktif Danışan", "#22c55e", true, false),
                    ("Pasif / Uygun Değil", "#ef4444", false, true)
                ]),
            new CustomerSeed(
                "VegaLife Diyetisyenlik",
                "vegalife-diyetisyenlik",
                "Danışan Planı",
                [
                    ("Yeni Talep", "#94a3b8", false, false),
                    ("Ön Değerlendirme", "#60a5fa", false, false),
                    ("Plan Hazırlandı", "#a78bfa", false, false),
                    ("Aktif Takip", "#22c55e", true, false),
                    ("Donduruldu / Kaybedildi", "#ef4444", false, true)
                ])
        };

        foreach (var customer in customers)
        {
            await SeedCustomerAsync(
                dbContext,
                adminUser.Id,
                adminEmail,
                allPermissions,
                customer,
                logger,
                cancellationToken);
        }
    }

    private static async Task SeedCustomerAsync(
        IApplicationDbContext dbContext,
        Guid adminUserId,
        string adminEmail,
        IReadOnlyCollection<Permission> allPermissions,
        CustomerSeed customer,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(x => x.Slug == customer.Slug, cancellationToken);
        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = customer.Name,
                Slug = customer.Slug,
                Email = adminEmail,
                Status = "active"
            };
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded customer {TenantSlug}", customer.Slug);
        }

        await EnsureTenantSettingsAsync(dbContext, tenant.Id, cancellationToken);
        var adminRole = await EnsureAdminRoleAsync(dbContext, tenant.Id, allPermissions, cancellationToken);
        await EnsureStaffRoleAsync(dbContext, tenant.Id, allPermissions, cancellationToken);
        await EnsureUserTenantAsync(dbContext, adminUserId, tenant.Id, cancellationToken);
        await EnsureUserRoleAsync(dbContext, adminUserId, adminRole.Id, cancellationToken);
        await SeedDemoPipelineAsync(dbContext, tenant.Id, customer, cancellationToken);
        await SeedDemoChannelsAsync(dbContext, tenant.Id, cancellationToken);
        await SeedDemoOffersAsync(dbContext, tenant.Id, customer.Slug, cancellationToken);
    }

    private static async Task EnsureTenantSettingsAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var settings = await dbContext.TenantSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (settings is not null)
        {
            return;
        }

        dbContext.TenantSettings.Add(new TenantSettings
        {
            TenantId = tenantId,
            Theme = "light",
            Language = "tr",
            Currency = "TRY",
            TimeZone = "Europe/Istanbul"
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Role> EnsureAdminRoleAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        IReadOnlyCollection<Permission> allPermissions,
        CancellationToken cancellationToken)
    {
        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Name == "Admin",
            cancellationToken);

        if (adminRole is null)
        {
            adminRole = new Role { TenantId = tenantId, Name = "Admin" };
            dbContext.Roles.Add(adminRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

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
        return adminRole;
    }

    private static readonly string[] StaffPermissionKeys =
    [
        "contacts.read", "contacts.create", "contacts.update", "contacts.delete",
        "companies.read", "companies.create", "companies.update",
        "users.read",
        "roles.read",
        "tags.read", "tags.create", "tags.update", "tags.delete",
        "pipelines.read",
        "deals.read", "deals.create", "deals.update", "deals.delete", "deals.move_stage",
        "notes.read", "notes.create", "notes.update", "notes.delete",
        "tasks.read", "tasks.create", "tasks.update", "tasks.delete",
        "activities.read", "activities.create", "activities.update",
        "appointments.read", "appointments.create", "appointments.update", "appointments.delete",
        "offers.read", "offers.create", "offers.update",
        "reports.read",
        "channels.read", "channels.create", "channels.update",
        "conversations.read", "conversations.create", "conversations.update",
        "conversations.assign", "conversations.update_status", "conversations.mark_read",
        "messages.read", "messages.send",
        "custom_fields.read", "custom_fields.create", "custom_fields.update",
        "notifications.read"
    ];

    private static async Task EnsureStaffRoleAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        IReadOnlyCollection<Permission> allPermissions,
        CancellationToken cancellationToken)
    {
        var staffRole = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.TenantId == tenantId && x.Name == "Uzman",
            cancellationToken);

        if (staffRole is null)
        {
            staffRole = new Role { TenantId = tenantId, Name = "Uzman" };
            dbContext.Roles.Add(staffRole);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var staffPermissionIds = allPermissions
            .Where(p => StaffPermissionKeys.Contains(p.Key))
            .Select(p => p.Id)
            .ToHashSet();

        var existingRolePermissions = await dbContext.RolePermissions
            .Where(x => x.RoleId == staffRole.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in staffPermissionIds.Where(id => !existingRolePermissions.Contains(id)))
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = staffRole.Id,
                PermissionId = permissionId
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureUserTenantAsync(
        IApplicationDbContext dbContext,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.UserTenants.AnyAsync(
            x => x.UserId == userId && x.TenantId == tenantId,
            cancellationToken);

        if (exists)
        {
            return;
        }

        var hasAnyTenant = await dbContext.UserTenants.AnyAsync(x => x.UserId == userId, cancellationToken);
        dbContext.UserTenants.Add(new UserTenant
        {
            UserId = userId,
            TenantId = tenantId,
            Status = "active",
            IsDefault = !hasAnyTenant
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureUserRoleAsync(
        IApplicationDbContext dbContext,
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var hasAdminRole = await dbContext.UserRoles.AnyAsync(
            x => x.UserId == userId && x.RoleId == roleId,
            cancellationToken);

        if (hasAdminRole)
        {
            return;
        }

        dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoPipelineAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        CustomerSeed customer,
        CancellationToken cancellationToken)
    {
        var pipelineExists = await dbContext.Pipelines.AnyAsync(x => x.TenantId == tenantId, cancellationToken);
        if (pipelineExists)
        {
            return;
        }

        var pipeline = new Pipeline { TenantId = tenantId, Name = customer.PipelineName, Order = 1 };
        dbContext.Pipelines.Add(pipeline);
        await dbContext.SaveChangesAsync(cancellationToken);

        var stages = customer.Stages.Select((stage, index) => new Stage
        {
            TenantId = tenantId,
            PipelineId = pipeline.Id,
            Name = stage.Name,
            Order = index + 1,
            Color = stage.Color,
            IsWinStage = stage.IsWinStage,
            IsLostStage = stage.IsLostStage
        }).ToArray();

        dbContext.Stages.AddRange(stages);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoChannelsAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var channelDefs = new[]
        {
            ("whatsapp", "WhatsApp"),
            ("internal", "Dahili Chat")
        };

        foreach (var (provider, name) in channelDefs)
        {
            var exists = await dbContext.Channels.AnyAsync(
                x => x.TenantId == tenantId && x.Provider == provider,
                cancellationToken);
            if (exists)
            {
                continue;
            }

            dbContext.Channels.Add(new Channel
            {
                TenantId = tenantId,
                Provider = provider,
                Name = name,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedDemoOffersAsync(
        IApplicationDbContext dbContext,
        Guid tenantId,
        string slug,
        CancellationToken cancellationToken)
    {
        if (await dbContext.OfferPackages.AnyAsync(x => x.TenantId == tenantId, cancellationToken))
        {
            return;
        }

        var packages = slug.Contains("vegalife", StringComparison.OrdinalIgnoreCase)
            ? new[]
            {
                new OfferPackage
                {
                    TenantId = tenantId,
                    Name = "Başlangıç Beslenme Paketi",
                    Description = "4 haftalık takip ve kişiselleştirilmiş plan",
                    SessionCount = 4,
                    DurationMinutes = 45,
                    Price = 4500,
                    Currency = "TRY",
                    Status = "active"
                },
                new OfferPackage
                {
                    TenantId = tenantId,
                    Name = "Yoğun Dönüşüm Paketi",
                    Description = "8 seans + haftalık check-in",
                    SessionCount = 8,
                    DurationMinutes = 45,
                    Price = 8000,
                    Currency = "TRY",
                    Status = "active"
                }
            }
            : new[]
            {
                new OfferPackage
                {
                    TenantId = tenantId,
                    Name = "4 Seans Paketi",
                    Description = "Bireysel danışmanlık başlangıç paketi",
                    SessionCount = 4,
                    DurationMinutes = 50,
                    Price = 6000,
                    Currency = "TRY",
                    Status = "active"
                },
                new OfferPackage
                {
                    TenantId = tenantId,
                    Name = "8 Seans Paketi",
                    Description = "Süreç odaklı devam paketi",
                    SessionCount = 8,
                    DurationMinutes = 50,
                    Price = 11000,
                    Currency = "TRY",
                    Status = "active"
                },
                new OfferPackage
                {
                    TenantId = tenantId,
                    Name = "Çift Terapisi Paketi",
                    Description = "6 seans çift danışmanlığı",
                    SessionCount = 6,
                    DurationMinutes = 60,
                    Price = 12000,
                    Currency = "TRY",
                    Status = "active"
                }
            };

        dbContext.OfferPackages.AddRange(packages);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record CustomerSeed(
        string Name,
        string Slug,
        string PipelineName,
        IReadOnlyCollection<(string Name, string Color, bool IsWinStage, bool IsLostStage)> Stages);
}
