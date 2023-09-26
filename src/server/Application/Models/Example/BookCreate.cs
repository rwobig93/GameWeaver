namespace Application.Models.Example;

public class BookCreate
{
    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Pages { get; set; }
}