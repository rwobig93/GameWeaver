namespace Application.Models.Example;

public class BookUpdate
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Author { get; set; }
    public int? Pages { get; set; }
}