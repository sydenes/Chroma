using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/contacts")]
public class ContactsController(IContactService contactService) : ControllerBase
{
    [RequirePermission("contacts.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ContactSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await contactService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("contacts.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await contactService.GetByIdAsync(id, cancellationToken);
        return contact is null
            ? NotFound(ApiResponse.Fail("contacts.notFound", "Contact not found."))
            : Ok(ApiResponse<ContactDto>.Ok(contact));
    }

    [RequirePermission("contacts.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(ApiResponse.Fail("contacts.firstNameRequired", "First name is required."));

        var contact = await contactService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = contact.Id }, ApiResponse<ContactDto>.Ok(contact));
    }

    [RequirePermission("contacts.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateContactRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return BadRequest(ApiResponse.Fail("contacts.firstNameRequired", "First name is required."));

        var contact = await contactService.UpdateAsync(id, request, cancellationToken);
        return contact is null
            ? NotFound(ApiResponse.Fail("contacts.notFound", "Contact not found."))
            : Ok(ApiResponse<ContactDto>.Ok(contact));
    }

    [RequirePermission("contacts.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await contactService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("contacts.deleted", "Contact deleted."))
            : NotFound(ApiResponse.Fail("contacts.notFound", "Contact not found."));
    }
}
