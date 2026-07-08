using Microsoft.Extensions.DependencyInjection;

namespace Chroma.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
