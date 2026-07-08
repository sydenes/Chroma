using Chroma.Application.Modules.Companies.Dtos;

namespace Chroma.Application.Modules.Companies.Services;

public interface ICompanyService
{
    Task<CompanySearchResult> SearchAsync(CompanySearchRequest request, CancellationToken cancellationToken);
    Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken);
    Task<CompanyDto?> UpdateAsync(Guid id, UpdateCompanyRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
