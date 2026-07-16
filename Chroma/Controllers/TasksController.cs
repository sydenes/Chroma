using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Tasks.Dtos;
using Chroma.Application.Modules.Tasks.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [RequirePermission("tasks.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] CrmTaskSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await taskService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("tasks.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, cancellationToken);
        return task is null
            ? NotFound(ApiResponse.Fail("Task not found."))
            : Ok(ApiResponse<CrmTaskDto>.Ok(task));
    }

    [RequirePermission("tasks.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateCrmTaskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Title is required."));

        var task = await taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = task.Id }, ApiResponse<CrmTaskDto>.Ok(task));
    }

    [RequirePermission("tasks.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateCrmTaskRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Title is required."));

        var task = await taskService.UpdateAsync(id, request, cancellationToken);
        return task is null
            ? NotFound(ApiResponse.Fail("Task not found."))
            : Ok(ApiResponse<CrmTaskDto>.Ok(task));
    }

    [RequirePermission("tasks.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Task deleted."))
            : NotFound(ApiResponse.Fail("Task not found."));
    }
}
