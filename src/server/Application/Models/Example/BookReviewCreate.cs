namespace Application.Models.Example;

public class BookReviewCreate
{
    public Guid BookId { get; set; }
    public string Author { get; set; } = null!;
    public string Content { get; set; } = null!;
}