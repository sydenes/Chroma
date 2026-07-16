using Chroma.Application.Modules.Offers.Dtos;

namespace Chroma.Application.Modules.Offers.Services;

public interface IOfferService
{
    Task<OfferPackageSearchResult> SearchAsync(OfferPackageSearchRequest request, CancellationToken cancellationToken);
    Task<OfferPackageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<OfferPackageDto> CreateAsync(CreateOfferPackageRequest request, CancellationToken cancellationToken);
    Task<OfferPackageDto?> UpdateAsync(Guid id, UpdateOfferPackageRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
