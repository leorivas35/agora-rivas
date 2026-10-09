namespace Core.Entities;

public class Music : BaseEntity
{
    public required string Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }
    public string? Format { get; set; }
}
