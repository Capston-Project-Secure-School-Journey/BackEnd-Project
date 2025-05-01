using Api.Domain.Models;
using Api.DTOs.NotificationService;
using AutoMapper;

namespace Api.Mappings;

public class NotificationProfile: Profile
{
    public NotificationProfile()
    {
        CreateMap<CreateNotificationDto, Notification>();
    }
}