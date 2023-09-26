namespace Application.Models.Example;

public class BookReviewUpdate
{
    public Guid BookId { get; set; }
    public string? Author { get; set; }
    public string? Content { get; set; }
}