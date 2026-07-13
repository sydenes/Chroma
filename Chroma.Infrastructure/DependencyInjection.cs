using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Companies.Services;
using Chroma.Application.Modules.Auth.Services;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Application.Modules.Roles.Services;
using Chroma.Application.Modules.Tenants.Services;
using Chroma.Application.Modules.Users.Services;
using Chroma.Application.Modules.Tags.Services;
using Chroma.Application.Modules.Pipelines.Services;
using Chroma.Application.Modules.Deals.Services;
using Chroma.Application.Modules.Notes.Services;
using Chroma.Application.Modules.Tasks.Services;
using Chroma.Application.Modules.Activities.Services;
using Chroma.Application.Modules.Channels.Services;
using Chroma.Application.Modules.Conversations.Services;
using Chroma.Application.Modules.Messages.Services;
using Chroma.Application.Modules.Forms.Services;
using Chroma.Application.Modules.CustomFields.Services;
using Chroma.Application.Modules.Files.Services;
using Chroma.Application.Modules.Workflows.Services;
using Chroma.Application.Modules.Notifications.Services;
using Chroma.Application.Modules.Reports.Services;
using Chroma.Infrastructure.Integrations;
using Chroma.Infrastructure.Options;
using Chroma.Infrastructure.Persistence;
using Chroma.Infrastructure.Security;
using Chroma.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chroma.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection was not found.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.Configure<IntegrationsOptions>(configuration.GetSection(IntegrationsOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUserService>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserService>());
        services.AddScoped<ICurrentTenant>(sp => sp.GetRequiredService<CurrentUserService>());

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IContactChannelService, ContactChannelService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IPipelineService, PipelineService>();
        services.AddScoped<IDealService, DealService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IFormService, FormService>();
        services.AddScoped<ICustomFieldService, CustomFieldService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddSingleton<IChannelSecretProtector, AesChannelSecretProtector>();
        services.AddSingleton<IChannelAdapter, WhatsAppAdapter>();
        services.AddSingleton<IChannelAdapter, MetaAdapter>();
        services.AddSingleton<IChannelAdapter, EmailAdapter>();
        services.AddSingleton<IChannelAdapter, TelegramAdapter>();
        services.AddSingleton<IChannelAdapterFactory, ChannelAdapterFactory>();

        return services;
    }
}
