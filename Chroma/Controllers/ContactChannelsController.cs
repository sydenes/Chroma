using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Contacts.Dtos;
using Chroma.Application.Modules.Contacts.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/contacts/{contactId:guid}/channels")]
public class ContactChannelsController(IContactChannelService contactChannelService) : ControllerBase
{
    [RequirePermission("contact_channels.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync(
        Guid contactId,
        [FromQuery] ContactChannelSearchRequest request,
        CancellationToken cancellationToken)
    {
        var searchRequest = new ContactChannelSearchRequest
        {
            ContactId = contactId,
            Query = request.Query,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var response = await contactChannelService.SearchAsync(searchRequest, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("contact_channels.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid contactId, Guid id, CancellationToken cancellationToken)
    {
        var channel = await contactChannelService.GetByIdAsync(contactId, id, cancellationToken);
        return channel is null
            ? NotFound(ApiResponse.Fail("Contact channel not found."))
            : Ok(ApiResponse<ContactChannelDto>.Ok(channel));
    }

    [RequirePermission("contact_channels.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid contactId,
        CreateContactChannelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChannelType) || string.IsNullOrWhiteSpace(request.Value))
            return BadRequest(ApiResponse.Fail("ChannelType and Value are required."));

        var createRequest = new CreateContactChannelRequest
        {
            TenantId = request.TenantId,
            ContactId = contactId,
            ChannelType = request.ChannelType,
            Value = request.Value,
            IsPrimary = request.IsPrimary
        };

        var channel = await contactChannelService.CreateAsync(createRequest, cancellationToken);
        return CreatedAtAction("GetById", new { contactId, id = channel.Id }, ApiResponse<ContactChannelDto>.Ok(channel));
    }

    [RequirePermission("contact_channels.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid contactId,
        Guid id,
        UpdateContactChannelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ChannelType) || string.IsNullOrWhiteSpace(request.Value))
            return BadRequest(ApiResponse.Fail("ChannelType and Value are required."));

        var channel = await contactChannelService.UpdateAsync(contactId, id, request, cancellationToken);
        return channel is null
            ? NotFound(ApiResponse.Fail("Contact channel not found."))
            : Ok(ApiResponse<ContactChannelDto>.Ok(channel));
    }

    [RequirePermission("contact_channels.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid contactId, Guid id, CancellationToken cancellationToken)
    {
        var deleted = await contactChannelService.DeleteAsync(contactId, id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Contact channel deleted."))
            : NotFound(ApiResponse.Fail("Contact channel not found."));
    }
}
