using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Workflows.Dtos;
using Chroma.Application.Modules.Workflows.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/workflows")]
public class WorkflowsController(IWorkflowService workflowService) : ControllerBase
{
    [RequirePermission("workflows.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] WorkflowSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await workflowService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("workflows.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var workflow = await workflowService.GetByIdAsync(id, cancellationToken);
        return workflow is null
            ? NotFound(ApiResponse.Fail("Workflow not found."))
            : Ok(ApiResponse<WorkflowDto>.Ok(workflow));
    }

    [RequirePermission("workflows.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var workflow = await workflowService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = workflow.Id }, ApiResponse<WorkflowDto>.Ok(workflow));
    }

    [RequirePermission("workflows.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var workflow = await workflowService.UpdateAsync(id, request, cancellationToken);
        return workflow is null
            ? NotFound(ApiResponse.Fail("Workflow not found."))
            : Ok(ApiResponse<WorkflowDto>.Ok(workflow));
    }

    [RequirePermission("workflows.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await workflowService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Workflow deleted."))
            : NotFound(ApiResponse.Fail("Workflow not found."));
    }
}
