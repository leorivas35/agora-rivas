namespace Core.Entities;

public class Library : BaseEntity
{    
    public required string Title { get; set; }

    public required string Author { get; set; }

    public required string Genre { get; set; }

    public required string Format { get; set; }

    public DateTimeOffset UploadedAt { get; set; }
    public string? UploaderId { get; set; }

    public Profile? Uploader { get; set; }

}
