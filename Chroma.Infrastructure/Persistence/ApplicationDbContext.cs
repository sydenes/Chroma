using Chroma.Application.Abstractions;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactChannel> ContactChannels => Set<ContactChannel>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContactTag> ContactTags => Set<ContactTag>();
    public DbSet<CompanyTag> CompanyTags => Set<CompanyTag>();
    public DbSet<Pipeline> Pipelines => Set<Pipeline>();
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<CrmTask> CrmTasks => Set<CrmTask>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormResponse> FormResponses => Set<FormResponse>();
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowTrigger> WorkflowTriggers => Set<WorkflowTrigger>();
    public DbSet<WorkflowCondition> WorkflowConditions => Set<WorkflowCondition>();
    public DbSet<WorkflowAction> WorkflowActions => Set<WorkflowAction>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<OfferPackage> OfferPackages => Set<OfferPackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
