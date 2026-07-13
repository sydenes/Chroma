using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.CustomFields.Dtos;
using Chroma.Application.Modules.CustomFields.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/custom-fields")]
public class CustomFieldsController(ICustomFieldService customFieldService) : ControllerBase
{
    [RequirePermission("custom_fields.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] CustomFieldSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await customFieldService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("custom_fields.read")]
    [HttpGet("values")]
    public async Task<IActionResult> GetValuesAsync([FromQuery] GetCustomFieldValuesRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType))
            return BadRequest(ApiResponse.Fail("EntityType is required."));

        var values = await customFieldService.GetValuesAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(values));
    }

    [RequirePermission("custom_fields.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var field = await customFieldService.GetByIdAsync(id, cancellationToken);
        return field is null
            ? NotFound(ApiResponse.Fail("Custom field not found."))
            : Ok(ApiResponse<CustomFieldDto>.Ok(field));
    }

    [RequirePermission("custom_fields.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.EntityType))
            return BadRequest(ApiResponse.Fail("Name and EntityType are required."));

        var field = await customFieldService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = field.Id }, ApiResponse<CustomFieldDto>.Ok(field));
    }

    [RequirePermission("custom_fields.set_value")]
    [HttpPost("values")]
    public async Task<IActionResult> SetValueAsync(SetCustomFieldValueRequest request, CancellationToken cancellationToken)
    {
        var value = await customFieldService.SetValueAsync(request, cancellationToken);
        return Ok(ApiResponse<CustomFieldValueDto>.Ok(value));
    }

    [RequirePermission("custom_fields.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCustomFieldRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var field = await customFieldService.UpdateAsync(id, request, cancellationToken);
        return field is null
            ? NotFound(ApiResponse.Fail("Custom field not found."))
            : Ok(ApiResponse<CustomFieldDto>.Ok(field));
    }

    [RequirePermission("custom_fields.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await customFieldService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Custom field deleted."))
            : NotFound(ApiResponse.Fail("Custom field not found."));
    }
}
