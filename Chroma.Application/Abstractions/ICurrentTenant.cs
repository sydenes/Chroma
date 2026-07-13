namespace Chroma.Application.Abstractions;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
}
