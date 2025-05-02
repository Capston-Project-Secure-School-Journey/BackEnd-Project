using System.Text.Json;

namespace Api.Common.Utilities;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}