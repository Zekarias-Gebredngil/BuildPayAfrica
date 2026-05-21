using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.IdentityService.Controllers;

[ApiController]
[Route("api/users/uploads")]
public sealed class UploadsController : ControllerBase
{
    private readonly IAmazonS3? _s3;
    private readonly IConfiguration _configuration;

    public UploadsController(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _s3 = serviceProvider.GetService<IAmazonS3>();
    }

    [HttpPost("presign")]
    [Authorize(Roles = "Contractor,Admin")]
    public ActionResult<object> Presign([FromBody] PresignRequest request)
    {
        var bucket = _configuration["S3:Bucket"];
        if (string.IsNullOrWhiteSpace(bucket) || _s3 is null)
        {
            return StatusCode(501, new { message = "S3 is not configured." });
        }

        var objectKey = $"identity/contractor-verification/{Guid.NewGuid()}_{request.FileName}";
        var presign = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            ContentType = request.ContentType
        };

        var url = _s3.GetPreSignedURL(presign);
        return Ok(new { url, objectKey, expiresInSeconds = 900 });
    }

    public sealed record PresignRequest(string FileName, string ContentType);
}

