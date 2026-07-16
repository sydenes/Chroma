using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Offers.Dtos;
using Chroma.Application.Modules.Offers.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/offers")]
public class OffersController(IOfferService offerService) : ControllerBase
{
    [RequirePermission("offers.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] OfferPackageSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await offerService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("offers.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var offer = await offerService.GetByIdAsync(id, cancellationToken);
        return offer is null
            ? NotFound(ApiResponse.Fail("Teklif paketi bulunamadı."))
            : Ok(ApiResponse<OfferPackageDto>.Ok(offer));
    }

    [RequirePermission("offers.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateOfferPackageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("İsim zorunludur."));

        if (request.SessionCount <= 0)
            return BadRequest(ApiResponse.Fail("Seans sayısı 0'dan büyük olmalıdır."));

        if (request.Price < 0)
            return BadRequest(ApiResponse.Fail("Fiyat negatif olamaz."));

        var offer = await offerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = offer.Id }, ApiResponse<OfferPackageDto>.Ok(offer));
    }

    [RequirePermission("offers.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateOfferPackageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("İsim zorunludur."));

        if (request.SessionCount <= 0)
            return BadRequest(ApiResponse.Fail("Seans sayısı 0'dan büyük olmalıdır."));

        if (request.Price < 0)
            return BadRequest(ApiResponse.Fail("Fiyat negatif olamaz."));

        var offer = await offerService.UpdateAsync(id, request, cancellationToken);
        return offer is null
            ? NotFound(ApiResponse.Fail("Teklif paketi bulunamadı."))
            : Ok(ApiResponse<OfferPackageDto>.Ok(offer));
    }

    [RequirePermission("offers.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await offerService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Teklif paketi silindi."))
            : NotFound(ApiResponse.Fail("Teklif paketi bulunamadı."));
    }
}
