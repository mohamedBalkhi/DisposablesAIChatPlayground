using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    [JsonPropertyName("content")]
    public required List<ChatContent> Content { get; set; }
}