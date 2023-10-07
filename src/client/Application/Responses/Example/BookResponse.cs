﻿namespace Application.Responses.Example;

public class BookResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;
    public int Pages { get; set; }
}