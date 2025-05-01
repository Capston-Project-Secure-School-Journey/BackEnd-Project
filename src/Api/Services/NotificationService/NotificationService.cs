using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Extensions;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.NotificationService;

public class NotificationService(
    Context context,
    IMapper mapper) : INotificationService
{
    private const int PageSize = 10;

    public async Task<Pagination<Notification>> GetNotifications(Guid recipientId, int currentPage)
    {
        var query = context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();
        var count = await query.CountAsync();
        var notifications = await query
            .Pagination(currentPage, PageSize)
            .ToListAsync();
        var response = new Pagination<Notification>(notifications, PageSize, currentPage, count);

        return response;
    }

    public async Task<int> NumberOfNotReadNotification(Guid recipientId)
    {
        return await context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .CountAsync();
    }

    public async Task<Notification> GetNotificationAsync(Guid notificationId)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null)
            throw new NotFoundException("Thông báo không tồn tại.");

        return notification;
    }

    public async Task<Notification> CreateNotification(CreateNotificationDto dto)
    {
        var notification = mapper.Map<Notification>(dto);
        notification.IsRead = false;
        notification.CreatedAt = DateTimeHelper.GetDateTimeUtc7();
        await context.Notifications.AddAsync(notification);
        await context.SaveChangesAsync();

        return notification;
    }

    public async Task MarkNotificationByRecipient(Guid recipientId)
    {
        var notifications = await context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            context.Notifications.Update(notification);
        }

        await context.SaveChangesAsync();
    }

    public async Task MarkNotification(Guid notificationId)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsRead);

        if (notification != null)
        {
            notification.IsRead = true;
            context.Notifications.Update(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task IsOwnerOfNotification(Guid notificationId, Guid userId)
    {
        var fact = await context.Notifications
            .Where(n => n.RecipientId == userId && n.Id == notificationId)
            .AnyAsync();

        if (!fact)
            throw new BadRequestException("Thông báo không tồn tại.");
    }
}