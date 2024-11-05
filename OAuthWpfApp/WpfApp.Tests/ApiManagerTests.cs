using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using WpfApp.Models;

namespace WpfApp.Tests
{
    public class ApiManagerTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly ApiManager _apiManager;

        public ApiManagerTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) 
            { 
                BaseAddress = new Uri("https://localhost:44349") 
            };
            _apiManager = new ApiManager(httpClient);
        }

        [Fact]
        public async Task RegisterUser_Success_ReturnsServerMessage()
        {
            // Arrange
            var user = new User { Username = "testuser", Password = "password123" };
            var expectedResponseContent = "Registration successful";
            
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponseContent)
                });

            // Act
            var result = await _apiManager.RegisterUser(user);

            // Assert
            Assert.Equal(expectedResponseContent, result);
        }

        [Fact]
        public async Task RegisterUser_Failure_ThrowsExceptionWithServerMessage()
        {
            // Arrange
            var user = new User { Username = "testuser", Password = "password123" };
            var expectedResponseContent = "User already exists";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(expectedResponseContent)
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _apiManager.RegisterUser(user));
            Assert.Contains(expectedResponseContent, exception.Message);
        }

        [Fact]
        public async Task AuthenticateUser_Success_ReturnsAuthorizationMessage()
        {
            // Arrange
            var user = new User { Username = "testuser", Password = "password123" };
            var authResponse = new AuthResponse { AccessToken = "access_token", RefreshToken = "refresh_token" };
            var responseContent = JsonSerializer.Serialize(authResponse);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _apiManager.AuthenticateUser(user);

            // Assert
            Assert.Equal("Successful authorization", result);
        }

        [Fact]
        public async Task AuthenticateUser_Failure_ThrowsExceptionWithServerMessage()
        {
            // Arrange
            var user = new User { Username = "testuser", Password = "wrong_password" };
            var expectedResponseContent = "Authentication failed: ";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent($"{{\"ErrorDescription\": \"{expectedResponseContent}\"}}")
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _apiManager.AuthenticateUser(user));
            Assert.Contains(expectedResponseContent, exception.Message);
        }

        [Fact]
        public async Task GetUserData_Success_ReturnsUserData()
        {
            // Arrange
            var userDataList = new List<UserData>
            {
                new UserData { Username = "user1", PasswordHash = "hash1" },
                new UserData { Username = "user2", PasswordHash = "hash2" }
            };
            var responseContent = JsonSerializer.Serialize(userDataList);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.PathAndQuery == "/users"), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _apiManager.GetUserData();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("user1", result[0].Username);
        }

        [Fact]
        public async Task GetUserData_Unauthorized_RefreshTokenAndRetry()
        {
            // Arrange
            var refreshResponse = new AuthResponse { AccessToken = "new_access_token", RefreshToken = "new_refresh_token" };
            var userDataList = new List<UserData> { new UserData { Username = "user1", PasswordHash = "hash1" } };

            _httpMessageHandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized }) // First request fails
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(refreshResponse)) }) // Refresh token
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(userDataList)) }); // Retry request

            // Act
            var result = await _apiManager.GetUserData();

            // Assert
            Assert.Single(result);
            Assert.Equal("user1", result[0].Username);
        }

        [Fact]
        public async Task RefreshToken_Failure_ThrowsExceptionWithServerMessage()
        {
            // Arrange
            var expectedResponseContent = "Token refresh failed: ";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent($"{{\"ErrorDescription\": \"{expectedResponseContent}\"}}")
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _apiManager.RefreshToken());
            Assert.Contains(expectedResponseContent, exception.Message);
        }
    }
}
