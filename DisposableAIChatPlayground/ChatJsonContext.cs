using System.Text.Json.Serialization;

namespace DisposableAIChatPlayground;

[JsonSerializable(typeof(ChatRequest))]
[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(ChatContent))]
[JsonSerializable(typeof(ChatResponse))]
[JsonSerializable(typeof(Choice))]
[JsonSerializable(typeof(ResponseChatMessage))]
public partial class ChatJsonContext : JsonSerializerContext
{
}