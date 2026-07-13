using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Reports.Dtos;
using Chroma.Application.Modules.Reports.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [RequirePermission("reports.read")]
    [HttpGet("pipeline-conversion")]
    public async Task<IActionResult> GetPipelineConversionAsync(
        [FromQuery] PipelineConversionReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GetPipelineConversionAsync(request, cancellationToken);
        return Ok(ApiResponse<PipelineConversionReportDto>.Ok(report));
    }

    [RequirePermission("reports.read")]
    [HttpGet("activity-volume")]
    public async Task<IActionResult> GetActivityVolumeAsync(
        [FromQuery] ActivityVolumeReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GetActivityVolumeAsync(request, cancellationToken);
        return Ok(ApiResponse<ActivityVolumeReportDto>.Ok(report));
    }
}
