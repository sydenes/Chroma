using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Channels.Dtos;
using Chroma.Application.Modules.Channels.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/channels")]
public class ChannelsController(IChannelService channelService) : ControllerBase
{
    [RequirePermission("channels.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ChannelSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await channelService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("channels.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var channel = await channelService.GetByIdAsync(id, cancellationToken);
        return channel is null
            ? NotFound(ApiResponse.Fail("Channel not found."))
            : Ok(ApiResponse<ChannelDto>.Ok(channel));
    }

    [RequirePermission("channels.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateChannelRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Provider))
            return BadRequest(ApiResponse.Fail("Name and Provider are required."));

        var channel = await channelService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = channel.Id }, ApiResponse<ChannelDto>.Ok(channel));
    }

    [RequirePermission("channels.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var channel = await channelService.UpdateAsync(id, request, cancellationToken);
        return channel is null
            ? NotFound(ApiResponse.Fail("Channel not found."))
            : Ok(ApiResponse<ChannelDto>.Ok(channel));
    }

    [RequirePermission("channels.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await channelService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Channel deleted."))
            : NotFound(ApiResponse.Fail("Channel not found."));
    }
}
