using Core.Constants;

namespace Core.Entities;

public class Image : BaseEntity
{
    public required string FileName { get; set; }        // original name, for display only
    public required string StoredFileName { get; set; }  // generated name (e.g., a GUID)
    public required ImageFormat Format { get; set; }        
    public required long SizeInBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; } 
    public required string StoragePath { get; set; }
    public required DateTimeOffset UploadedAt { get; set; }
    public required string UploaderId { get; set; }
    public required Profile Uploader { get; set; }
}
