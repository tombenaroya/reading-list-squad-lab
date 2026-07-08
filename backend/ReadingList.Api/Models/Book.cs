namespace ReadingList.Api.Models;

public sealed class Book
{
    public const int MaxNotesLength = 1000;

    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Author { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Unread;

    public string? Notes { get; set; }
}
