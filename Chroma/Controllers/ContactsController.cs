using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[ApiController]
[Route("api/contacts")]
public class ContactsController(IContactService contactService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ContactSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await contactService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await contactService.GetByIdAsync(id, cancellationToken);
        return contact is null
            ? NotFound(ApiResponse.Fail("Contact not found."))
            : Ok(ApiResponse<ContactDto>.Ok(contact));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(ApiResponse.Fail("FirstName is required."));

        var contact = await contactService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = contact.Id }, ApiResponse<ContactDto>.Ok(contact));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(ApiResponse.Fail("FirstName is required."));

        var contact = await contactService.UpdateAsync(id, request, cancellationToken);
        return contact is null
            ? NotFound(ApiResponse.Fail("Contact not found."))
            : Ok(ApiResponse<ContactDto>.Ok(contact));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await contactService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Contact deleted."))
            : NotFound(ApiResponse.Fail("Contact not found."));
    }
}

