using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Companies.Dtos;
using Chroma.Application.Modules.Companies.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController(ICompanyService companyService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] CompanySearchRequest request, CancellationToken cancellationToken)
    {
        var response = await companyService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await companyService.GetByIdAsync(id, cancellationToken);
        return company is null
            ? NotFound(ApiResponse.Fail("Company not found."))
            : Ok(ApiResponse<CompanyDto>.Ok(company));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var company = await companyService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = company.Id }, ApiResponse<CompanyDto>.Ok(company));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var company = await companyService.UpdateAsync(id, request, cancellationToken);
        return company is null
            ? NotFound(ApiResponse.Fail("Company not found."))
            : Ok(ApiResponse<CompanyDto>.Ok(company));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await companyService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Company deleted."))
            : NotFound(ApiResponse.Fail("Company not found."));
    }
}
