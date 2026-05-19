using System;

namespace BuildPay.Application.DTOs;

public sealed class ProjectPhotoDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
}

