using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace DisposableAIChatPlayground;

public static class Program
{
    private static readonly ConsoleColor UserColor = ConsoleColor.Cyan;
    // ReSharper disable once InconsistentNaming
    private static readonly ConsoleColor AIColor = ConsoleColor.Green;
    private static readonly ConsoleColor SystemColor = ConsoleColor.Yellow;
    private static readonly List<(string Role, string Content)> ChatHistory = new(); // Currently not used. but could be used!

    public static async Task Main(string[] args)
    {
        // Load Env File
        // Env.Load(options: new DotNetEnv.LoadOptions(LoadOptions.TraversePath()));
        Env.Load(options: LoadOptions.TraversePath());
        Console.WriteLine();
        WriteColoredText("🤖 Welcome to the Interactive Chat Console! 🤖", SystemColor);
        WriteColoredText("Type your messages and chat with the AI.", SystemColor);
        WriteColoredText("Type 'exit' to quit, 'clear' to clear chat history.", SystemColor);
        Console.WriteLine();

        // Create a logger factory for better error handling
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            // Set minimum log level to avoid showing debug info in console
            builder.SetMinimumLevel(LogLevel.Warning)
                  .AddConsole();
        });
        var logger = loggerFactory.CreateLogger<ChatService>();

        // Initialize the chat service with your API key
        // Load API key from environment variables or user secrets
        string apiUrl = Environment.GetEnvironmentVariable("OPENROUTER_API_URL") ?? "https://openrouter.ai/api/v1/";
        string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                        ?? throw new InvalidOperationException(
                            "API key not found. Set the OPENROUTER_API_KEY environment variable.");
        string modelName = Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ?? "meta-llama/llama-4-maverick:free";
        using ChatService service = new(apiUrl, apiKey, modelName, logger);

        while (true)
        {
            // Display prompt and get user input
            WriteColoredText("You: ", UserColor, newLine: false);
            string userInput = Console.ReadLine() ?? string.Empty;

            // Handle special commands
            if (string.IsNullOrWhiteSpace(userInput)) continue;
            if (userInput.ToLower() == "exit") break;
            if (userInput.ToLower() == "clear")
            {
                ChatHistory.Clear();
                Console.Clear();
                WriteColoredText("Chat history cleared!", SystemColor);
                continue;
            }

            // Add user message to history
            ChatHistory.Add(("user", userInput));

            try
            {
                // Show "thinking" animation
                WriteColoredText("AI: ", AIColor, newLine: false);
                var thinkingCancellation = new CancellationTokenSource();
                _ = ShowThinkingAnimation(thinkingCancellation.Token); // Fire and Forget (We'll cancel after we get the response!) if we await here we will end up having it infinitely.

                // Send message to API
                var response = await service.SendAsync(userInput);

                // Stop thinking animation and display response
                await thinkingCancellation.CancelAsync();
                await Task.Delay(100); // Give animation time to stop

                Console.SetCursorPosition(4, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth - 5));
                Console.SetCursorPosition(4, Console.CursorTop);

                // Display AI response with word wrapping
                WriteWrappedText(response, AIColor, 4);
                Console.WriteLine();

                // Add AI response to history
                ChatHistory.Add(("assistant", response));
            }
            catch (Exception ex)
            {
                await Task.Delay(100); // Ensure thinking animation stops
                Console.WriteLine();
                WriteColoredText($"Error: {ex.Message}", ConsoleColor.Red);
            }
        }

        WriteColoredText("Thank you for chatting! Goodbye! 👋", SystemColor);
    }

    private static void WriteColoredText(string text, ConsoleColor color, bool newLine = true)
    {
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(text);
        else
            Console.Write(text);
        Console.ResetColor();
    }

    private static void WriteWrappedText(string text, ConsoleColor color, int indent = 0)
    {
        Console.ForegroundColor = color;

        int maxWidth = Console.WindowWidth - indent - 1;
        int currentLinePosition = indent;

        foreach (var word in text.Split(' '))
        {
            // Check if this word would exceed the line width
            if (currentLinePosition + word.Length + 1 > maxWidth)
            {
                Console.WriteLine(); // New line
                Console.Write(new string(' ', indent)); // Indent
                currentLinePosition = indent;
            }

            Console.Write(word + " ");
            currentLinePosition += word.Length + 1;
        }

        Console.ResetColor();
    }

    private static Task ShowThinkingAnimation(CancellationToken cancellationToken)
    {
        var animationChars = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        int animationIndex = 0;

        // Run animation in a separate task
        return Task.Run(async () =>
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Write(animationChars[animationIndex]);
                    animationIndex = (animationIndex + 1) % animationChars.Length;
                    await Task.Delay(100, cancellationToken);
                    Console.Write("\b");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }, cancellationToken: cancellationToken);
    }
}