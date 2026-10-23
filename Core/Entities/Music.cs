namespace Core.Entities;

public class Music : BaseEntity
{    
    public required string Genre { get; set; }

    public DateTimeOffset UploadedAt { get; set; }

    public string? UploadById { get; set; }

    public Profile? UploadBy { get; set; }

    public required string Path { get; set; }
}
