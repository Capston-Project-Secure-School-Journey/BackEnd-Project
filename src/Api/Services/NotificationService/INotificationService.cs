using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.TransferDTOs.Responses;

namespace Api.Services.NotificationService;

public interface INotificationService
{
    Task<Pagination<Notification>> GetNotifications(Guid recipientId, int currentPage);
    Task<int> NumberOfNotReadNotification(Guid recipientId);
    Task<Notification> GetNotificationAsync(Guid notificationId);
    Task<Notification> CreateNotification(CreateNotificationDto dto);
    Task MarkNotificationByRecipient(Guid recipientId);
    Task MarkNotification(Guid notificationId);
}