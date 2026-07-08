using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Companies.Services;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Infrastructure.Persistence;
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

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
