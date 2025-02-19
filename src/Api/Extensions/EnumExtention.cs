using System.ComponentModel.DataAnnotations;
using System.Reflection;
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
                DisplayText = status.ToString(),
                Value = status
            })
            .ToList();
    }
    
    private static string GetEnumDisplayName<T>(T value) where T : Enum
    {
        return value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttribute<DisplayAttribute>()?
            .Name ?? value.ToString();
    }
}