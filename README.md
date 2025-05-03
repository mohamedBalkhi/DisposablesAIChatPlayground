# Disposable AI Chat Playground

A simple console application demonstrating the use of disposable patterns in C# with an AI chat interface.

## Introduction

This project serves as a tutorial on implementing the `IDisposable` pattern in C#, which is crucial for proper resource management. The application creates an interactive chat console that connects to an AI service (OpenRouter API).

### Understanding Disposables in C#

In C#, the `IDisposable` interface provides a mechanism for releasing unmanaged resources. When working with resources like file handles, network connections, or database connections, it's important to release these resources properly when they're no longer needed.

Key concepts demonstrated in this project:

- Implementing the `IDisposable` interface
- Using the `using` statement for automatic disposal
- Proper resource cleanup patterns

### HttpClient and Disposable Pattern

This project specifically demonstrates how to properly manage `HttpClient` instances. While `HttpClient` implements `IDisposable`, it's designed for reuse and shouldn't be disposed after each request (to avoid socket exhaustion). In this example, we create a single `HttpClient` instance in our `ChatService` class and dispose of it when the service is no longer needed.

> **Note:** This is an educational example and may not follow all best practices for production applications. In real-world applications, consider using `HttpClientFactory` for better management of `HttpClient` instances.

## Getting Started

### Prerequisites

- .NET 6.0 or later
- An OpenRouter API key

### Setup

1. Clone this repository
2. Copy the `.env.example` file to create your own `.env` file:
3. Edit the `.env` file with your OpenRouter API key:
   You can get an API key by signing up at [OpenRouter](https://openrouter.ai/).

4. Optionally, change the model name to use a different AI model available on OpenRouter.

### Running the Application

Navigate to the project directory and run:
`dotnet run --project DisposableAIChatPlayground`

## Usage

- Type messages to chat with the AI
- Type `clear` to clear the chat history
- Type `exit` to quit the application


## License

This project is licensed under the MIT License - see the LICENSE file for details.