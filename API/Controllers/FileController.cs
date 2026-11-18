using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Core.Constants;
using Core.Interfaces;
using Core.Entities;
using API.DTOs;

using API.Helpers;
namespace API.Controllers;

[Route("api/files")]
//[EnableRateLimiting(AppDefinition.RateLimitPolicy)]
public class FileController(IUnitOfWork unitOfWork, IFileService fileService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AgoraFile>>> GetDocuments()
    {
        IReadOnlyList<AgoraFile> documents = await unitOfWork.Repository<AgoraFile>().ListAllAsync();
        return Ok(documents);
    }
    [HttpPost("create")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [RequestSizeLimit(AppDefinition.MaxFileSize)] // Limit request size to 10 MB
    [RequestFormLimits(MultipartBodyLengthLimit = AppDefinition.MaxFileSize)]
    public async Task<ActionResult<FileDto>> CreateFile([FromForm] FileRequestDto request, CancellationToken cancellationToken)
    {
        if (request.File.Length > AppDefinition.MaxFileSize)
            return StatusCode(StatusCodes.Status413PayloadTooLarge, "File exceeds the 10 MB limit.");

        (bool isValid, string? errorMessage, string? fileExtension) = await FileValidator.ValidateAsync(request.File);
        if (!isValid)
            return BadRequest(new ProblemDetails { Title = "Invalid file", Detail = errorMessage, Status = StatusCodes.Status400BadRequest });

        // Never use the client-provided file name directly for storage or processing.
        string safeFileName = $"{Guid.NewGuid()}{fileExtension}";

        await using var Stream = request.File.OpenReadStream();
        AgoraFile doc = await fileService.CreateFileAsync(
            request.Title,
            request.File.ContentType,
            Path.GetExtension(request.File.FileName),
            Stream,
            cancellationToken
            );

        return CreatedAtAction(nameof(GetFileById), new { id = doc.Id }, doc);
    }
    [HttpGet("{id}")]
    public IActionResult GetFileById(int id)
    {
        return Ok();
    }
    [HttpPut("{id}")]
    public IActionResult UpdateFile(int id)
    {
        return Ok();
    }
    [HttpDelete("{id}")]
    public IActionResult DeleteFile(int id)
    {
        return Ok();
    }


}