using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace TransparentClock
{
    /// <summary>
    /// HTTP client for QuickLink URL shortening API.
    /// Supports both POST (primary) and GET (fallback) request methods.
    /// Handles authentication, request/response serialization, and error handling.
    /// Thread-safe for use in WinForms UI context (all operations are async).
    /// </summary>
    public class QuickLinkApiClient
    {
        private const string BaseUrl = "https://qlynk.vercel.app/api/v1/st";
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;
        private bool useGetFallback = false;

        /// <summary>
        /// Initializes a new instance of the QuickLinkApiClient.
        /// Creates a dedicated HttpClient with appropriate timeout.
        /// </summary>
        public QuickLinkApiClient()
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        /// <summary>
        /// Initializes a new instance with an existing HttpClient.
        /// Useful for testing and dependency injection.
        /// </summary>
        /// <param name="customHttpClient">A preconfigured HttpClient instance. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if customHttpClient is null.</exception>
        public QuickLinkApiClient(HttpClient customHttpClient)
        {
            if (customHttpClient == null)
                throw new ArgumentNullException(nameof(customHttpClient));

            httpClient = customHttpClient;
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Asynchronously shortens a long URL using the QuickLink API.
        /// 
        /// This method:
        /// - Validates inputs (throws on null/empty apiKey or longUrl)
        /// - Sends a POST request with Bearer token authentication
        /// - Parses success (200) responses into QuickLinkSuccessResponse
        /// - Parses error responses into QuickLinkErrorResponse
        /// - Provides specific error messages for common status codes:
        ///   - 401 Unauthorized: Invalid or expired API key
        ///   - 403 Forbidden: API key lacks permission
        ///   - 409 Conflict: Alias already exists
        ///   - 429 Too Many Requests: Rate limit exceeded
        /// - Throws QuickLinkApiException on HTTP/network errors
        /// 
        /// Usage:
        /// <code>
        /// var client = new QuickLinkApiClient();
        /// try
        /// {
        ///     var result = await client.ShortenUrlAsync(
        ///         apiKey: "your-api-key",
        ///         longUrl: "https://github.com/deepdeyiitgn/Clock-Overlays",
        ///         alias: "clock-app"
        ///     );
        ///     MessageBox.Show($"Shortened: {result.ShortenedUrl}");
        /// }
        /// catch (QuickLinkApiException ex)
        /// {
        ///     MessageBox.Show($"Error: {ex.Message}", "Failed to Shorten URL");
        /// }
        /// </code>
        /// </summary>
        /// <param name="apiKey">The QuickLink API key for authentication. Must not be null or empty.</param>
        /// <param name="longUrl">The long URL to shorten. Must not be null or empty.</param>
        /// <param name="alias">Optional custom alias for the shortened URL. Can be null (auto-generated).</param>
        /// <returns>A QuickLinkSuccessResponse if the request succeeds.</returns>
        /// <exception cref="ArgumentNullException">Thrown if apiKey or longUrl is null.</exception>
        /// <exception cref="ArgumentException">Thrown if apiKey or longUrl is empty.</exception>
        /// <exception cref="QuickLinkApiException">Thrown on HTTP errors or network failures.</exception>
        public async Task<QuickLinkSuccessResponse> ShortenUrlAsync(
            string apiKey,
            string longUrl,
            string? alias = null)
        {
            // Validate inputs
            if (apiKey == null)
                throw new ArgumentNullException(nameof(apiKey));

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key cannot be empty", nameof(apiKey));

            if (longUrl == null)
                throw new ArgumentNullException(nameof(longUrl));

            if (string.IsNullOrWhiteSpace(longUrl))
                throw new ArgumentException("URL cannot be empty", nameof(longUrl));

            // Create request body
            var requestBody = new
            {
                longUrl,
                alias
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                // Try POST request first (primary method)
                if (!useGetFallback)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
                        {
                            Content = content,
                            Headers =
                            {
                                { "Authorization", $"Bearer {apiKey}" }
                            }
                        };

                        var response = await httpClient.SendAsync(request);
                        return await HandleResponse(response, apiKey, longUrl);
                    }
                    catch (Exception ex)
                    {
                        // If POST fails, fallback to GET (for debugging/compatibility)
                        if (ex is HttpRequestException or TimeoutException or TaskCanceledException)
                        {
                            useGetFallback = true;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Fallback to GET request if POST failed
                {
                    var aliasParam = string.IsNullOrWhiteSpace(alias) ? "" : $"&alias={HttpUtility.UrlEncode(alias)}";
                    var getUrl = $"{BaseUrl}?api={HttpUtility.UrlEncode(apiKey)}&url={HttpUtility.UrlEncode(longUrl)}{aliasParam}&format=json";

                    var request = new HttpRequestMessage(HttpMethod.Get, getUrl);
                    var response = await httpClient.SendAsync(request);
                    return await HandleResponse(response, apiKey, longUrl);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new QuickLinkApiException(
                    "Network error while communicating with QuickLink API",
                    innerException: ex
                );
            }
            catch (TaskCanceledException)
            {
                throw new QuickLinkApiException("Request timeout (30 seconds) while shortening URL");
            }
        }

        private async Task<QuickLinkSuccessResponse> HandleResponse(
            HttpResponseMessage response,
            string apiKey,
            string longUrl)
        {
            var responseJson = await response.Content.ReadAsStringAsync();

            // Handle success status codes
            if (response.IsSuccessStatusCode)
            {
                // Try multiple property name variations
                var successResponse = TryParseSuccessResponse(responseJson);
                if (successResponse != null)
                    return successResponse;

                throw new QuickLinkApiException("Failed to parse successful response from API");
            }

            // Handle error status codes
            string errorMessage = GetErrorMessageForStatusCode((int)response.StatusCode);

            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<QuickLinkErrorResponse>(
                    responseJson,
                    jsonOptions
                );

                if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.Message))
                {
                    errorMessage = $"{errorMessage}: {errorResponse.Message}";
                }
            }
            catch
            {
                // If error parsing fails, use generic error message
            }

            throw new QuickLinkApiException(
                errorMessage,
                (int)response.StatusCode
            );
        }

        private QuickLinkSuccessResponse? TryParseSuccessResponse(string json)
        {
            try
            {
                // Attempt standard deserialization
                var response = JsonSerializer.Deserialize<QuickLinkSuccessResponse>(
                    json,
                    jsonOptions
                );

                if (response != null && !string.IsNullOrWhiteSpace(response.ShortenedUrl))
                    return response;

                // Try parsing as JsonElement for flexible field mapping
                using (var doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;

                    // Extract shortened URL from various possible field names
                    var shortenedUrl = ExtractStringField(root, "shortenedUrl", "shortened_url", "short_url", "url");
                    var directUrl = ExtractStringField(root, "directRedirectUrl", "direct_redirect_url", "redirectUrl", "redirect_url");

                    if (string.IsNullOrWhiteSpace(shortenedUrl))
                        return null;

                    return new QuickLinkSuccessResponse
                    {
                        ShortenedUrl = shortenedUrl,
                        DirectRedirectUrl = directUrl ?? shortenedUrl,
                        LongUrl = string.Empty,
                        Alias = ExtractStringField(root, "alias") ?? "",
                        RemainingCredits = ExtractIntField(root, "remainingCredits", "remaining_credits") ?? 0,
                        ExpiresAt = ExtractDateTimeField(root, "expiresAt", "expires_at")
                    };
                }
            }
            catch
            {
                return null;
            }
        }

        private string? ExtractStringField(JsonElement element, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                if (element.TryGetProperty(fieldName, out var prop) && prop.ValueKind == JsonValueKind.String)
                {
                    return prop.GetString();
                }
            }

            return null;
        }

        private int? ExtractIntField(JsonElement element, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                if (element.TryGetProperty(fieldName, out var prop) && prop.ValueKind == JsonValueKind.Number)
                {
                    if (prop.TryGetInt32(out var intValue))
                        return intValue;
                }
            }

            return null;
        }

        private DateTime? ExtractDateTimeField(JsonElement element, params string[] fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                if (element.TryGetProperty(fieldName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.String && DateTime.TryParse(prop.GetString(), out var dt))
                        return dt;
                }
            }

            return null;
        }

        /// <summary>
        /// Maps HTTP status codes to user-friendly error messages for students.
        /// </summary>
        private string GetErrorMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "❌ Invalid request. Please check your URL.",
                401 => "❌ API key is missing",
                403 => "❌ Invalid / expired API key or credits exhausted",
                409 => "❌ Alias already taken",
                429 => "❌ Too many requests. Please wait and try again.",
                500 => "❌ Server error. Please try again later.",
                503 => "❌ Service temporarily unavailable. Please try again later.",
                _ => $"❌ Error (HTTP {statusCode}). Please try again."
            };
        }
    }

    /// <summary>
    /// Exception thrown when the QuickLink API request fails.
    /// Provides detailed error information including status code and API error status.
    /// </summary>
    public class QuickLinkApiException : Exception
    {
        /// <summary>
        /// The HTTP status code from the API response, if available.
        /// Null if the error occurred before receiving a response.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// The error status code from the QuickLink API (e.g., "INVALID_URL", "RATE_LIMIT_EXCEEDED").
        /// Null if the response could not be parsed or did not include a status.
        /// </summary>
        public string? ApiStatus { get; set; }

        /// <summary>
        /// Initializes a new instance with a message.
        /// </summary>
        public QuickLinkApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance with a message and inner exception.
        /// </summary>
        public QuickLinkApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance with message, HTTP status code, and API status.
        /// </summary>
        public QuickLinkApiException(string message, int statusCode, string? apiStatus = null)
            : base(message)
        {
            StatusCode = statusCode;
            ApiStatus = apiStatus;
        }
    }
}
