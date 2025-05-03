using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

public class ChatResponse
{
    [JsonPropertyName("choices")]
    public required List<Choice> Choices { get; set; }
}