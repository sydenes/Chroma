using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Appointments.Dtos;
using Chroma.Application.Modules.Appointments.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentsController(IAppointmentService appointmentService) : ControllerBase
{
    [RequirePermission("appointments.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await appointmentService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("appointments.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await appointmentService.GetByIdAsync(id, cancellationToken);
        return appointment is null
            ? NotFound(ApiResponse.Fail("Randevu bulunamadı."))
            : Ok(ApiResponse<AppointmentDto>.Ok(appointment));
    }

    [RequirePermission("appointments.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Başlık zorunludur."));

        if (request.EndsAtUtc <= request.StartsAtUtc)
            return BadRequest(ApiResponse.Fail("Bitiş zamanı başlangıç zamanından sonra olmalıdır."));

        var appointment = await appointmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = appointment.Id }, ApiResponse<AppointmentDto>.Ok(appointment));
    }

    [RequirePermission("appointments.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Başlık zorunludur."));

        if (request.EndsAtUtc <= request.StartsAtUtc)
            return BadRequest(ApiResponse.Fail("Bitiş zamanı başlangıç zamanından sonra olmalıdır."));

        var appointment = await appointmentService.UpdateAsync(id, request, cancellationToken);
        return appointment is null
            ? NotFound(ApiResponse.Fail("Randevu bulunamadı."))
            : Ok(ApiResponse<AppointmentDto>.Ok(appointment));
    }

    [RequirePermission("appointments.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await appointmentService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Randevu silindi."))
            : NotFound(ApiResponse.Fail("Randevu bulunamadı."));
    }
}
