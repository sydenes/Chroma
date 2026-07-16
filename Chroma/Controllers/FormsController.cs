using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Forms.Dtos;
using Chroma.Application.Modules.Forms.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/forms")]
public class FormsController(IFormService formService) : ControllerBase
{
    [RequirePermission("forms.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] FormSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await formService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("forms.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var form = await formService.GetByIdAsync(id, cancellationToken);
        return form is null
            ? NotFound(ApiResponse.Fail("Form not found."))
            : Ok(ApiResponse<FormDto>.Ok(form));
    }

    [RequirePermission("forms.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateFormRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var form = await formService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = form.Id }, ApiResponse<FormDto>.Ok(form));
    }

    [RequirePermission("forms.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateFormRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var form = await formService.UpdateAsync(id, request, cancellationToken);
        return form is null
            ? NotFound(ApiResponse.Fail("Form not found."))
            : Ok(ApiResponse<FormDto>.Ok(form));
    }

    [RequirePermission("forms.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await formService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Form deleted."))
            : NotFound(ApiResponse.Fail("Form not found."));
    }

    [RequirePermission("forms.create_field")]
    [HttpPost("{formId:guid}/fields")]
    public async Task<IActionResult> CreateFieldAsync(
        Guid formId,
        CreateFormFieldRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Label))
            return BadRequest(ApiResponse.Fail("Name and Label are required."));

        var field = await formService.CreateFieldAsync(formId, request, cancellationToken);
        return Ok(ApiResponse<FormFieldDto>.Ok(field));
    }

    [RequirePermission("forms.update_field")]
    [HttpPut("{formId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> UpdateFieldAsync(
        Guid formId,
        Guid fieldId,
        UpdateFormFieldRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Label))
            return BadRequest(ApiResponse.Fail("Name and Label are required."));

        var field = await formService.UpdateFieldAsync(formId, fieldId, request, cancellationToken);
        return field is null
            ? NotFound(ApiResponse.Fail("Form field not found."))
            : Ok(ApiResponse<FormFieldDto>.Ok(field));
    }

    [RequirePermission("forms.delete_field")]
    [HttpDelete("{formId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> DeleteFieldAsync(Guid formId, Guid fieldId, CancellationToken cancellationToken)
    {
        var deleted = await formService.DeleteFieldAsync(formId, fieldId, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Form field deleted."))
            : NotFound(ApiResponse.Fail("Form field not found."));
    }
}
