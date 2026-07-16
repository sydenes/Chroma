using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Pipelines.Dtos;
using Chroma.Application.Modules.Pipelines.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/pipelines")]
public class PipelinesController(IPipelineService pipelineService) : ControllerBase
{
    [RequirePermission("pipelines.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] PipelineSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await pipelineService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("pipelines.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var pipeline = await pipelineService.GetByIdAsync(id, cancellationToken);
        return pipeline is null
            ? NotFound(ApiResponse.Fail("Pipeline not found."))
            : Ok(ApiResponse<PipelineDto>.Ok(pipeline));
    }

    [RequirePermission("pipelines.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreatePipelineRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var pipeline = await pipelineService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = pipeline.Id }, ApiResponse<PipelineDto>.Ok(pipeline));
    }

    [RequirePermission("pipelines.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdatePipelineRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var pipeline = await pipelineService.UpdateAsync(id, request, cancellationToken);
        return pipeline is null
            ? NotFound(ApiResponse.Fail("Pipeline not found."))
            : Ok(ApiResponse<PipelineDto>.Ok(pipeline));
    }

    [RequirePermission("pipelines.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await pipelineService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Pipeline deleted."))
            : NotFound(ApiResponse.Fail("Pipeline not found."));
    }

    [RequirePermission("pipelines.create_stage")]
    [HttpPost("{pipelineId:guid}/stages")]
    public async Task<IActionResult> CreateStageAsync(
        Guid pipelineId,
        CreateStageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var stage = await pipelineService.CreateStageAsync(pipelineId, request, cancellationToken);
        return Ok(ApiResponse<StageDto>.Ok(stage));
    }

    [RequirePermission("pipelines.update_stage")]
    [HttpPut("{pipelineId:guid}/stages/{stageId:guid}")]
    public async Task<IActionResult> UpdateStageAsync(
        Guid pipelineId,
        Guid stageId,
        UpdateStageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var stage = await pipelineService.UpdateStageAsync(pipelineId, stageId, request, cancellationToken);
        return stage is null
            ? NotFound(ApiResponse.Fail("Stage not found."))
            : Ok(ApiResponse<StageDto>.Ok(stage));
    }

    [RequirePermission("pipelines.delete_stage")]
    [HttpDelete("{pipelineId:guid}/stages/{stageId:guid}")]
    public async Task<IActionResult> DeleteStageAsync(Guid pipelineId, Guid stageId, CancellationToken cancellationToken)
    {
        var deleted = await pipelineService.DeleteStageAsync(pipelineId, stageId, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Stage deleted."))
            : NotFound(ApiResponse.Fail("Stage not found."));
    }
}
