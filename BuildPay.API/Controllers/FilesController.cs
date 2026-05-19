using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FilesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<object>> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0)
        {
            return BadRequest(new { message = "Empty file." });
        }

        var ext = Path.GetExtension(file.FileName);
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".pdf" };
        if (!allowed.Contains(ext))
        {
            return BadRequest(new { message = "Unsupported file type." });
        }

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploadsRoot, fileName);

        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream, cancellationToken);

        var url = $"/uploads/{fileName}";
        return Ok(new { url });
    }
}

