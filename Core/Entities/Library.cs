namespace Core.Entities;

public class Library : BaseEntity
{
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
}
