using System;
using BuildPay.Domain.Enums;

namespace BuildPay.Application.DTOs;

public sealed class ProjectDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? ContractorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; }
}

