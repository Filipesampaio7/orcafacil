using FluentValidation;
using OrcaFacil.Application.DTOs.Services;

namespace OrcaFacil.Application.Validators.Services;

public class CreateServiceValidator : AbstractValidator<CreateServiceDto>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EstimatedDurationMinutes).GreaterThan(0)
            .When(x => x.EstimatedDurationMinutes.HasValue);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}

public class UpdateServiceValidator : AbstractValidator<UpdateServiceDto>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DefaultPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EstimatedDurationMinutes).GreaterThan(0)
            .When(x => x.EstimatedDurationMinutes.HasValue);
        RuleFor(x => x.Category).MaximumLength(100);
        RuleFor(x => x.Status).IsInEnum();
    }
}
