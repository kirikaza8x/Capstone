
namespace AI.Application.Features.ChatBot.DTOs;

public record StreamChatRequestDto
{
    public string Message { get; init; } = string.Empty;
}

public record ChatMessageDto
{
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
