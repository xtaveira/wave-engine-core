using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Microwave.Tests.Integration
{
    public class AuthenticationConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthenticationConfigurationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Should_Have_JWT_Authentication_Configured()
        {
            using var scope = _factory.Services.CreateScope();
            var authenticationSchemeProvider = scope.ServiceProvider
                .GetRequiredService<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();

            var defaultScheme = await authenticationSchemeProvider.GetDefaultAuthenticateSchemeAsync();

            Assert.NotNull(defaultScheme);
            Assert.Equal(JwtBearerDefaults.AuthenticationScheme, defaultScheme.Name);
        }
        [Fact]
        public async Task Should_Return_Unauthorized_For_Protected_Endpoint_Without_Token()
        {
            var response = await _client.GetAsync("/api/microwave/heating/status");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Should_Allow_Anonymous_Auth_Endpoints()
        {
            var loginRequest = new
            {
                username = "nonexistent",
                password = "wrong"
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/auth/login", content);

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Should_Accept_Valid_JWT_Token()
        {
            var configureRequest = new
            {
                username = "admin",
                password = "admin123"
            };

            var configJson = JsonSerializer.Serialize(configureRequest);
            var configContent = new StringContent(configJson, Encoding.UTF8, "application/json");

            var configResponse = await _client.PostAsync("/api/auth/configure", configContent);
            Assert.True(configResponse.IsSuccessStatusCode);

            var loginRequest = new
            {
                username = "admin",
                password = "admin123"
            };

            var loginJson = JsonSerializer.Serialize(loginRequest);
            var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            Assert.True(loginResponse.IsSuccessStatusCode);

            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<JsonElement>(loginResponseContent);

            var dataElement = loginResult.GetProperty("data");
            var token = dataElement.GetProperty("token").GetString();

            Assert.NotNull(token);
            Assert.NotEmpty(token);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var protectedResponse = await _client.GetAsync("/api/microwave/programs/predefined");

            if (!protectedResponse.IsSuccessStatusCode)
            {
                var errorContent = await protectedResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Protected endpoint failed. Status: {protectedResponse.StatusCode}, Content: {errorContent}");
            }

            Assert.True(protectedResponse.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Should_Reject_Invalid_JWT_Token()
        {
            var invalidToken = "invalid.jwt.token";

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

            var response = await _client.GetAsync("/api/microwave/heating/status");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Should_Reject_Expired_JWT_Token()
        {
            var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6ImFkbWluIiwidXNlcm5hbWUiOiJhZG1pbiIsIm5iZiI6MTY0MDk5NTIwMCwiZXhwIjoxNjQwOTk1MjAwLCJpYXQiOjE2NDA5OTUyMDB9.invalid";

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", expiredToken);

            var response = await _client.GetAsync("/api/microwave/heating/status");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
