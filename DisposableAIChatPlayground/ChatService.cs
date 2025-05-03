using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DisposableAIChatPlayground;

public class ChatService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatService>? _logger;
    private bool _disposed;
    private readonly string _modelName;

    public ChatService(string url, string bearerToken,string modelName, ILogger<ChatService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));
        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new ArgumentNullException(nameof(bearerToken));
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentNullException(nameof(modelName));

        _logger = logger;
        _modelName = modelName;

        try
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/"),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }
        catch (UriFormatException ex)
        {
            throw new ArgumentException("Invalid URL format.", nameof(url), ex);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);
    }

    public async Task<string> SendAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentNullException(nameof(message));

        // Create the payload with lowercase property names
        var payload = new ChatRequest
        {
            Model = _modelName,
            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "user",
                    Content = new List<ChatContent>
                    {
                        new ChatContent
                        {
                            Type = "text",
                            Text = message
                        }
                    }
                }
            }
        };

        // Serialize to JSON using source-generated context
        var jsonPayload = JsonSerializer.Serialize(payload, ChatJsonContext.Default.ChatRequest);
        _logger?.LogInformation("Request Payload: {Payload}", jsonPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        // Send the request
        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger?.LogError("API request failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode, errorContent);
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger?.LogInformation("Response: {Response}", responseContent);
            return GetResponseContent(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "HTTP request failed.");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger?.LogError(ex, "Request timed out.");
            throw new TimeoutException("API request timed out.", ex);
        }
    }

    private string GetResponseContent(string responseJson)
    {
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            _logger?.LogWarning("Response JSON is empty.");
            return string.Empty;
        }

        try
        {
            var chatResponse = JsonSerializer.Deserialize(responseJson, ChatJsonContext.Default.ChatResponse);
            var content = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(content))
            {
                _logger?.LogWarning("No content found in response.");
                return string.Empty;
            }
            return content;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to deserialize response JSON.");
            throw new InvalidOperationException("Failed to parse API response.", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}