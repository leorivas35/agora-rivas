namespace Core.Entities;

public class Image : BaseEntity
{
    public required string Format { get; set; }

    public required string Path { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public string? UploaderId { get; set; }

    public Profile? Uploader { get; set; }
}
