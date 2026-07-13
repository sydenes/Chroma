using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Contact> Contacts { get; }
    DbSet<ContactChannel> ContactChannels { get; }
    DbSet<Company> Companies { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<TenantSettings> TenantSettings { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Tag> Tags { get; }
    DbSet<ContactTag> ContactTags { get; }
    DbSet<CompanyTag> CompanyTags { get; }
    DbSet<Pipeline> Pipelines { get; }
    DbSet<Stage> Stages { get; }
    DbSet<Deal> Deals { get; }
    DbSet<Note> Notes { get; }
    DbSet<CrmTask> CrmTasks { get; }
    DbSet<Activity> Activities { get; }
    DbSet<Channel> Channels { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationParticipant> ConversationParticipants { get; }
    DbSet<Message> Messages { get; }
    DbSet<Form> Forms { get; }
    DbSet<FormField> FormFields { get; }
    DbSet<FormResponse> FormResponses { get; }
    DbSet<CustomField> CustomFields { get; }
    DbSet<CustomFieldValue> CustomFieldValues { get; }
    DbSet<StoredFile> StoredFiles { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowTrigger> WorkflowTriggers { get; }
    DbSet<WorkflowCondition> WorkflowConditions { get; }
    DbSet<WorkflowAction> WorkflowActions { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
