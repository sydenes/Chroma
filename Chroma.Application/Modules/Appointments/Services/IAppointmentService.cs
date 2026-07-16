using Chroma.Application.Modules.Appointments.Dtos;

namespace Chroma.Application.Modules.Appointments.Services;

public interface IAppointmentService
{
    Task<AppointmentSearchResult> SearchAsync(AppointmentSearchRequest request, CancellationToken cancellationToken);
    Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);
    Task<AppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
