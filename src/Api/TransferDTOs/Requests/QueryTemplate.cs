using System.Text.Json.Serialization;

namespace Api.Transfers.Requests;
public class QueryTemplate
{
    public int Limit { get; set; } = 15;
    public int Page { get; set; } = 1;
    public string SortBy { get; set; } = "Id";
    public string Direction { get; set; } = "DESC";
}