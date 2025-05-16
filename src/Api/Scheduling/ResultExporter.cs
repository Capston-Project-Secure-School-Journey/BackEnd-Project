using System.Text.Json;
using Api.Domain.Models;
using Api.DTOs.Scheduling;
namespace Api.Scheduling;

public static class ResultExporter
{
    private const string BasePath = "/results";
    public static void Export(Dictionary<DriverData, List<Student>> results)
    {
        var simplifiedDict = results.ToDictionary(
            kvp => kvp.Key.Id.ToString(),
            kvp => kvp.Value
        );
        
        var json = JsonSerializer.Serialize(simplifiedDict, new JsonSerializerOptions { WriteIndented = true , MaxDepth = 10, ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,});
        var rnd = new Random().Next(1000, 999999999);
        var fileName = $"students_{rnd}.json";
        var currentDir = Path.Join(AppContext.BaseDirectory, BasePath);
        Directory.CreateDirectory(currentDir);
        File.WriteAllText(Path.Join(currentDir, fileName), json);
    }
}