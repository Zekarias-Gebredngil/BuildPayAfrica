using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using FluentValidation;
using MediatR;

namespace BuildPay.Application.Products.Commands;

public record CreateProductCommand(
    Guid SupplierId,
    string Name,
    decimal Price,
    int Quantity,
    Guid CategoryId,
    string ImageUrl) : IRequest<Guid>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Price).GreaterThan(0);
        RuleFor(v => v.Quantity).GreaterThanOrEqualTo(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public CreateProductCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var entity = new Product
        {
            Id = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl
        };

        _context.Products.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
