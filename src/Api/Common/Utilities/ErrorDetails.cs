using System.Text.Json;

namespace Api.Common.Utilities
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ValidationError> ValidationErrors { get; set; } = [];
        
        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}