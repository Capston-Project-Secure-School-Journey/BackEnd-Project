using Api.Common.Utilities.Exceptions;
using Api.Domain;
using Api.Domain.Models;
using Api.DTOs.NotificationService;
using Api.Extensions;
using Api.TransferDTOs.Responses;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.NotificationService;

public class NotificationService : INotificationService
{
    private readonly Context _context;
    private readonly IMapper _mapper;
    private const int PageSize = 10;

    public NotificationService(Context context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Pagination<Notification>> GetNotifications(Guid recipientId, int currentPage)
    {
        var count = await NumberOfNotReadNotification(recipientId);
        var notifications = await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Pagination(currentPage, PageSize)
            .ToListAsync();
        var response = new Pagination<Notification>(notifications, PageSize, currentPage, count);

        return response;
    }

    public async Task<int> NumberOfNotReadNotification(Guid recipientId)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .CountAsync();
    }

    public async Task<Notification> GetNotificationAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsRead);

        if (notification == null)
            throw new NotFoundException("Thông báo không tồn tại.");

        return notification;
    }

    public async Task<Notification> CreateNotification(CreateNotificationDto dto)
    {
        var notification = _mapper.Map<Notification>(dto);
        notification.IsRead = false;
        notification.CreatedAt = DateTimeHelper.GetDateTimeUtc7();
        await _context.Notifications.AddAsync(notification);
        _context.Entry(notification).State = EntityState.Added;
        await _context.SaveChangesAsync();
        
        return notification;
    }

    public async Task MarkNotificationByRecipient(Guid recipientId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            _context.Entry(notification).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkNotification(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsRead);

        if (notification != null)
        {
            notification.IsRead = true;
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}