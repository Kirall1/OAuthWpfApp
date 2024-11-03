using System.Text.Json.Serialization;

namespace WpfApp.Models;

public class ErrorResponse
{
    [JsonPropertyName("error")] public string Error { get; set; }

    [JsonPropertyName("error_description")] public string ErrorDescription { get; set; }
}