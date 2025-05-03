using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class ChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }
    [JsonPropertyName("messages")]
    public required List<ChatMessage> Messages { get; set; }
}