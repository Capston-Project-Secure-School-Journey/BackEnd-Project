using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Api.Attributes;
using Api.DTOs;

namespace Api.Extensions;

public static class EnumExtension
{
    public static List<ComboBoxItem> GetComboBoxItems<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(status => new ComboBoxItem
            {
                Name = GetEnumDisplayName(status),
                Id = Convert.ToInt16(status)
            })
            .ToList();
    }

    public static string GetEnumDisplayName<T>(T value) where T : Enum
    {
        return value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<DisplayAttribute>()?
            .Name ?? value.ToString();
    }

    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute != null ? attribute.Description : value.ToString();
    }

    public static int GetBanAttemptLimit(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<BanAttemptLimitAttribute>();
        if (attribute != null)
            return Convert.ToInt32(attribute.Limit);
        else
            return 0;
    }

    public static int GetBanAttemptBanTime(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<BanAttemptLimitAttribute>();
        if (attribute != null)
            return Convert.ToInt32(attribute.BanTime);
        else
            return 24 * 60 * 60;
    }
    
    public static int GetBanAttemptObservationWindow(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<BanAttemptLimitAttribute>();
        if (attribute != null)
            return Convert.ToInt32(attribute.ObservationWindow);
        else
            return 24;
    }
}