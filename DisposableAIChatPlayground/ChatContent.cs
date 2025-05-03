using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class ChatContent
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}