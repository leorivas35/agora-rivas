namespace Core.Entities;

public class Images : BaseEntity
{
    public required string Url { get; set; }
    public string? AltText { get; set; }
    public required string Caption { get; set; }
}
