using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Common.Exceptions;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace Api.Services;

public class GoogleMapsService(HttpClient httpClient, IConfiguration configuration)
{
    private readonly string _apiKey =
        configuration["GoogleMaps:ApiKey"] ?? throw new InvalidConfigurationException("Google Maps API key not found");
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public class GeocodeResponse
    {
        public string Status { get; set; }
        public List<GeocodeResult> Results { get; set; }
    }

    public class GeocodeResult
    {
        public string FormattedAddress { get; set; }
        public List<string> Types { get; set; }
        public Geometry Geometry { get; set; }
    }

    public class Geometry
    {
        public Location Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class DirectionsResponse
    {
        public string Status { get; set; }
        public List<Route> Routes { get; set; }
    }

    public class Route
    {
        public List<Leg> Legs { get; set; }
        [JsonPropertyName("overview_polyline")]
        public OverviewPolyline OverviewPolyline { get; set; }
        [JsonPropertyName("waypoint_order")]
        public List<int> WaypointOrder { get; set; }
    }

    public class OverviewPolyline
    {
        public string Points { get; set; }
    }

    public class Leg
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
        [JsonPropertyName("start_address")]
        public string StartAddress { get; set; }
        [JsonPropertyName("end_address")]
        public string EndAddress { get; set; }
        [JsonPropertyName("start_location")]
        public Location StartLocation { get; set; }
        [JsonPropertyName("end_location")]
        public Location EndLocation { get; set; }
        public List<Step> Steps { get; set; }
    }

    public class Distance
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class Duration
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }

    public class Step
    {
        public Distance Distance { get; set; }
        public Duration Duration { get; set; }
        [JsonPropertyName("start_address")]
        public string StartAddress { get; set; }
        [JsonPropertyName("end_address")]
        public string EndAddress { get; set; }
        [JsonPropertyName("start_location")]
        public Location StartLocation { get; set; }
        [JsonPropertyName("end_location")]
        public Location EndLocation { get; set; }
        [JsonPropertyName("html_instructions")]
        public string HtmlInstructions { get; set; }
        [JsonPropertyName("polyline")]
        public PolyLine PolyLine { get; set; }
        public string Maneuver { get; set; }
    }

    public class PolyLine
    {
        public string Points { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public async Task<(double lat, double lng)> GetLatLngFromAddressAsync(string address)
    {
        var url =
            $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

        var response = await httpClient.GetFromJsonAsync<GeocodeResponse>(url);

        var location = response?.Results.FirstOrDefault()?.Geometry.Location;
        if (location == null)
            throw new BadRequestException("Địa chỉ không hợp lệ.");

        return (location.Lat, location.Lng);
    }

    public async Task<bool> IsCarAccessibleAddressAsync(string address)
    {
        try
        {
            var url =
                $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";

            var response = await httpClient.GetFromJsonAsync<GeocodeResponse>(url);

            var result = response?.Results.FirstOrDefault();

            if (result == null)
                return false;

            var types = result.Types;

            return types.Contains("route") || types.Contains("street_address") || types.Contains("premise");
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<DirectionsResponse> GetOptimizedRouteAsync(
        string origin,
        string destination,
        List<string> waypoints)
    {
        var waypointsStr = "optimize:true";
        if (waypoints.Count > 0)
        {
            waypointsStr += "|" + string.Join("|", waypoints.Select(Uri.EscapeDataString));
        }

        var url =
            $"https://maps.googleapis.com/maps/api/directions/json?origin={Uri.EscapeDataString(origin)}&destination={Uri.EscapeDataString(destination)}&waypoints={waypointsStr}&key={_apiKey}";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonString = await httpClient.GetStringAsync(url);
        var response = JsonSerializer.Deserialize<DirectionsResponse>(jsonString, options);


        if (response == null || response.Status != "OK" || response.Routes.Count == 0)
            throw new InvalidDataException("");

        return response;
    }
}