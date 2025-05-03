using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class Choice
{
    [JsonPropertyName("message")]
    public required ResponseChatMessage Message { get; set; }
}