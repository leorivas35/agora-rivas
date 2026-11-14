using Core.Constants;

namespace API.DTOs;

public class DocumentDto
{
    public required string FileName { get; set; }
    public required string Title { get; set; }
    public required string Author { get; set; }
    public required string ContentType { get; set; }   // "image/png", "application/pdf"
    public required long SizeInBytes { get; set; }
    public required string StoragePath { get; set; }   // or a blob URL
    public required DocumentType Type { get; set; }
}