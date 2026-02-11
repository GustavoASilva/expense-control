using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpenseControl.Api.Features.Auth.Services;

public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public record GoogleUserInfo(string GoogleId, string Email, string Name, string? PictureUrl);

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GoogleTokenValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("Google");
            // Note: Using Google's tokeninfo endpoint is the standard approach for server-side token validation.
            // The token is sent as a query parameter as per Google's API specification.
            var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google token validation failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenInfo = JsonSerializer.Deserialize<GoogleTokenInfo>(content);

            if (tokenInfo == null)
            {
                _logger.LogWarning("Failed to deserialize Google token response");
                return null;
            }

            var expectedClientId = _configuration["Authentication:Google:ClientId"];
            if (string.IsNullOrWhiteSpace(expectedClientId))
            {
                _logger.LogError("Google ClientId is not configured");
                return null;
            }

            if (tokenInfo.Aud != expectedClientId)
            {
                _logger.LogWarning("Token audience mismatch. Expected: {Expected}, Got: {Actual}", expectedClientId, tokenInfo.Aud);
                return null;
            }

            if (string.IsNullOrWhiteSpace(tokenInfo.Sub) ||
                string.IsNullOrWhiteSpace(tokenInfo.Email) ||
                string.IsNullOrWhiteSpace(tokenInfo.Name))
            {
                _logger.LogWarning("Token is missing required claims");
                return null;
            }

            return new GoogleUserInfo(tokenInfo.Sub, tokenInfo.Email, tokenInfo.Name, tokenInfo.Picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }

    private class GoogleTokenInfo
    {
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("aud")]
        public string Aud { get; set; } = string.Empty;
    }
}
