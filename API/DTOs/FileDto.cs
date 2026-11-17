using Core.Constants;
using Core.Entities;

namespace API.DTOs;

public class FileDto
{
    public string Title { get; set; } = default!;    
    public string ContentType { get; set; } = default!;
    public string Extension { get; set; } = default!;
    public long SizeInBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
     public DocumentContent Content { get; set; } = default!;
  
}