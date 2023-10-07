namespace Application.Responses.v1.Example;

public class BookReviewResponse
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string Author { get; set; } = null!;
    public string Content { get; set; } = null!;
}