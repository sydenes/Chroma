using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Deals.Dtos;
using Chroma.Application.Modules.Deals.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/deals")]
public class DealsController(IDealService dealService) : ControllerBase
{
    [RequirePermission("deals.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] DealSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await dealService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("deals.read")]
    [HttpGet("board")]
    public async Task<IActionResult> GetBoardAsync([FromQuery] Guid pipelineId, CancellationToken cancellationToken)
    {
        var board = await dealService.GetBoardAsync(pipelineId, cancellationToken);
        return board is null
            ? NotFound(ApiResponse.Fail("Pipeline not found."))
            : Ok(ApiResponse<DealBoardDto>.Ok(board));
    }

    [RequirePermission("deals.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var deal = await dealService.GetByIdAsync(id, cancellationToken);
        return deal is null
            ? NotFound(ApiResponse.Fail("Deal not found."))
            : Ok(ApiResponse<DealDto>.Ok(deal));
    }

    [RequirePermission("deals.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateDealRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Title is required."));

        var deal = await dealService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = deal.Id }, ApiResponse<DealDto>.Ok(deal));
    }

    [RequirePermission("deals.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateDealRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Title is required."));

        var deal = await dealService.UpdateAsync(id, request, cancellationToken);
        return deal is null
            ? NotFound(ApiResponse.Fail("Deal not found."))
            : Ok(ApiResponse<DealDto>.Ok(deal));
    }

    [RequirePermission("deals.move_stage")]
    [HttpPost("{id:guid}/move-stage")]
    public async Task<IActionResult> MoveStageAsync(Guid id, MoveDealStageRequest request, CancellationToken cancellationToken)
    {
        var deal = await dealService.MoveStageAsync(id, request, cancellationToken);
        return deal is null
            ? NotFound(ApiResponse.Fail("Deal not found."))
            : Ok(ApiResponse<DealDto>.Ok(deal));
    }

    [RequirePermission("deals.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await dealService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Deal deleted."))
            : NotFound(ApiResponse.Fail("Deal not found."));
    }
}
