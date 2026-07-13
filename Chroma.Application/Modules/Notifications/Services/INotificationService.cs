using Chroma.Application.Modules.Notifications.Dtos;

namespace Chroma.Application.Modules.Notifications.Services;

public interface INotificationService
{
    Task<NotificationSearchResult> SearchAsync(NotificationSearchRequest request, CancellationToken cancellationToken);
    Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
    Task<NotificationDto?> MarkAsReadAsync(Guid id, MarkNotificationReadRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
