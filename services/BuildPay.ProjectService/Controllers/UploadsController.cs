using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.ProjectService.Controllers;

[ApiController]
[Route("api/projects/uploads")]
[Authorize(Roles = "Customer,Contractor,Admin")]
public sealed class UploadsController : ControllerBase
{
    private readonly IAmazonS3 _s3;
    private readonly IConfiguration _configuration;

    public UploadsController(IAmazonS3 s3, IConfiguration configuration)
    {
        _s3 = s3;
        _configuration = configuration;
    }

    [HttpPost("presign")]
    public ActionResult<object> Presign([FromBody] PresignRequest request)
    {
        var bucket = _configuration["S3:Bucket"];
        if (string.IsNullOrWhiteSpace(bucket))
        {
            return StatusCode(501, new { message = "S3 is not configured." });
        }

        var objectKey = $"project/photos/{Guid.NewGuid()}_{request.FileName}";
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

