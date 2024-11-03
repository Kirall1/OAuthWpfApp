using System.Text.Json.Serialization;

namespace WpfApp.Models;

public class UserData
{
    [JsonPropertyName("userName")]
    public string Username { get; set; }
    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; }
}