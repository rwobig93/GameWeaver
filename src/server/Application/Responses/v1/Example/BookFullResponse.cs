namespace Application.Responses.v1.Example;

public class BookFullResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Pages { get; set; }
    public List<BookReviewResponse> Reviews { get; set; } = new();
    public List<BookGenreResponse> Genres { get; set; } = new();
}