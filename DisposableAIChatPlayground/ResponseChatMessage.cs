using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class ResponseChatMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}