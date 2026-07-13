using Chroma.Application.Modules.Reports.Dtos;

namespace Chroma.Application.Modules.Reports.Services;

public interface IReportService
{
    Task<PipelineConversionReportDto> GetPipelineConversionAsync(PipelineConversionReportRequest request, CancellationToken cancellationToken);
    Task<ActivityVolumeReportDto> GetActivityVolumeAsync(ActivityVolumeReportRequest request, CancellationToken cancellationToken);
}
