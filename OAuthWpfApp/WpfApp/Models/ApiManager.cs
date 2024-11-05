using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WpfApp.Models
{
    public class ApiManager
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _refreshToken;

        public ApiManager()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7199") };
        }
        
        public ApiManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> RegisterUser(User user)
        {
            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/connect/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return responseContent;
                
                throw new Exception($"Registration failed: {responseContent}");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Cannot connect to server");
            }
        }

        public async Task<string> AuthenticateUser(User user)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", user.Username),
                new KeyValuePair<string, string>("password", user.Password)
            });

            try
            {
                var response = await _httpClient.PostAsync("/connect/token", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                    _accessToken = result.AccessToken;
                    _refreshToken = result.RefreshToken;
                    return "Successful authorization";
                }
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                throw new Exception($"Authentication failed: {errorResponse.ErrorDescription}");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Cannot connect to server");
            }
        }

        public async Task<List<UserData>> GetUserData()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            try
            {
                var response = await _httpClient.GetAsync("/users");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<List<UserData>>(responseContent);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await RefreshToken();
                    response = await _httpClient.GetAsync("/users");
                    responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                        return JsonSerializer.Deserialize<List<UserData>>(responseContent);
                }
                
                throw new Exception($"Failed to retrieve user data: {responseContent}");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Cannot connect to server");
            }
        }

        public async Task RefreshToken()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", _refreshToken),
            });
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");


            var response = await _httpClient.PostAsync("/connect/refresh", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                _accessToken = result.AccessToken;
                _refreshToken = result.RefreshToken;
            }
            else
            {
                var result = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                throw new Exception($"Token refresh failed: {result.ErrorDescription}");
            }
        }
    }
}
