using System.ComponentModel.DataAnnotations;
namespace API.DTOs;

public class FileRequestDto
{
    public required IFormFile File { get; set; }

    // Optional metadata sent alongside the file
    [Required, StringLength(200)]
    public string Title { get; set; } = default!;
}