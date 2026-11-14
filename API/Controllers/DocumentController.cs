using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Core.Constants;
using Core.Interfaces;
using Core.Entities;
namespace API.Controllers;

[EnableRateLimiting(ApiConst.RateLimitPolicy)]
public class DocumentController(IUnitOfWork unitOfWork) : BaseController
{
    [HttpGet]
    public IActionResult GetDocuments()
    {
        return Ok();
    }
    [HttpPost]
    public IActionResult CreateDocument()
    {
        return Ok();
    }
    [HttpGet("{id}")]
    public IActionResult GetDocumentById(int id)
    {
        return Ok();
    }
    [HttpPut("{id}")]
    public IActionResult UpdateDocument(int id)
    {
        return Ok();
    }
    [HttpDelete("{id}")]
    public IActionResult DeleteDocument(int id)
    {
        return Ok();
    }
    

}