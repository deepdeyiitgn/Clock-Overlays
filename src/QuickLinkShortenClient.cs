using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TransparentClock
{
    public sealed class QuickLinkShortenClient
    {
        private const string BaseUrl = "https://qlynk.vercel.app/api/v1/st";
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public QuickLinkShortenClient(HttpClient? httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<QuickLinkShortenResult> ShortenAsync(string apiKey, string longUrl, string? alias)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "MISSING_API_KEY",
                    Message = "API key missing",
                    StatusCode = 401
                });
            }

            if (string.IsNullOrWhiteSpace(longUrl))
            {
                return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "INVALID_URL",
                    Message = "Long URL is required",
                    StatusCode = null
                });
            }

            try
            {
                var payload = new
                {
                    longUrl,
                    alias = string.IsNullOrWhiteSpace(alias) ? null : alias
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload, jsonOptions),
                    Encoding.UTF8,
                    "application/json"
                );

                var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl)
                {
                    Content = content
                };

                // CRITICAL: Force JSON response
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                using var response = await httpClient.SendAsync(request).ConfigureAwait(false);
                string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // === CRITICAL CHECK: Validate JSON before parsing ===
                if (string.IsNullOrWhiteSpace(responseText) || !responseText.TrimStart().StartsWith("{"))
                {
                    // Not JSON - likely HTML error page or redirect
                    return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                    {
                        Code = "API_ERROR",
                        Message = "API returned non-JSON response. Check endpoint and credentials.",
                        StatusCode = (int)response.StatusCode
                    });
                }

                // === Parse JSON safely ===
                QuickLinkApiResponse? apiResponse;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<QuickLinkApiResponse>(responseText, jsonOptions);
                }
                catch (JsonException)
                {
                    return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                    {
                        Code = "INVALID_RESPONSE",
                        Message = "API response format is invalid",
                        StatusCode = (int)response.StatusCode
                    });
                }

                if (apiResponse == null)
                {
                    return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                    {
                        Code = "INVALID_RESPONSE",
                        Message = "Could not parse API response",
                        StatusCode = (int)response.StatusCode
                    });
                }

                // === Handle response status codes ===
                if (!response.IsSuccessStatusCode)
                {
                    return HandleErrorResponse((int)response.StatusCode, apiResponse);
                }

                // === Success ===
                if (string.IsNullOrEmpty(apiResponse.ShortenedUrl))
                {
                    return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                    {
                        Code = "INVALID_RESPONSE",
                        Message = "API did not return shortened URL",
                        StatusCode = 200
                    });
                }

                return QuickLinkShortenResult.Ok(new QuickLinkShortenSuccess
                {
                    Status = apiResponse.Status ?? "success",
                    ShortenedUrl = apiResponse.ShortenedUrl,
                    DirectRedirectUrl = apiResponse.DirectRedirectUrl,
                    LongUrl = apiResponse.LongUrl ?? longUrl,
                    Alias = apiResponse.Alias ?? alias,
                    RemainingCredits = apiResponse.RemainingCredits ?? 0,
                    ExpiresAt = apiResponse.ExpiresAt
                });
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "NETWORK_ERROR",
                    Message = "Network error. Check internet connection.",
                    StatusCode = null
                });
            }
            catch (TaskCanceledException)
            {
                return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "TIMEOUT",
                    Message = "Request timed out. Try again.",
                    StatusCode = null
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                return QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "UNKNOWN_ERROR",
                    Message = "Unexpected error occurred",
                    StatusCode = null
                });
            }
        }

        /// <summary>
        /// Convert HTTP status codes to user-friendly messages
        /// </summary>
        private QuickLinkShortenResult HandleErrorResponse(int statusCode, QuickLinkApiResponse response)
        {
            return statusCode switch
            {
                400 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "BAD_REQUEST",
                    Message = "Invalid request. Check URL format.",
                    StatusCode = 400
                }),
                401 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "UNAUTHORIZED",
                    Message = "API key missing or invalid",
                    StatusCode = 401
                }),
                403 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "FORBIDDEN",
                    Message = "Invalid or expired API key. Credits may be exhausted.",
                    StatusCode = 403
                }),
                409 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "CONFLICT",
                    Message = "Alias already taken. Try a different one.",
                    StatusCode = 409
                }),
                429 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "RATE_LIMITED",
                    Message = "Rate limit exceeded. Try again later.",
                    StatusCode = 429
                }),
                500 or 502 or 503 => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "SERVER_ERROR",
                    Message = "API server error. Try again later.",
                    StatusCode = statusCode
                }),
                _ => QuickLinkShortenResult.Fail(new QuickLinkShortenError
                {
                    Code = "API_ERROR",
                    Message = $"API error (code {statusCode})",
                    StatusCode = statusCode
                })
            };
        }
    }

    /// <summary>
    /// API Response model - matches actual QuickLink API response
    /// </summary>
    public class QuickLinkApiResponse
    {
        public string? Status { get; set; }
        public string? ShortenedUrl { get; set; }
        public string? DirectRedirectUrl { get; set; }
        public string? LongUrl { get; set; }
        public string? Alias { get; set; }
        public int? RemainingCredits { get; set; }
        public string? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Success response
    /// </summary>
    public class QuickLinkShortenSuccess
    {
        public string? Status { get; set; }
        public string? ShortenedUrl { get; set; }
        public string? DirectRedirectUrl { get; set; }
        public string? LongUrl { get; set; }
        public string? Alias { get; set; }
        public int RemainingCredits { get; set; }
        public string? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Error response
    /// </summary>
    public class QuickLinkShortenError
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public int? StatusCode { get; set; }
    }

    /// <summary>
    /// Result wrapper
    /// </summary>
    public class QuickLinkShortenResult
    {
        public bool IsSuccess { get; set; }
        public QuickLinkShortenSuccess? Success { get; set; }
        public QuickLinkShortenError? Error { get; set; }

        public static QuickLinkShortenResult Ok(QuickLinkShortenSuccess success)
        {
            return new QuickLinkShortenResult
            {
                IsSuccess = true,
                Success = success,
                Error = null
            };
        }

        public static QuickLinkShortenResult Fail(QuickLinkShortenError error)
        {
            return new QuickLinkShortenResult
            {
                IsSuccess = false,
                Success = null,
                Error = error
            };
        }
    }

    // Keep backward compatibility types if needed
    public class QuickLinkShortenErrorResponse { }
}
