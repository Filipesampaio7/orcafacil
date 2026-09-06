using FluentValidation;
using OrcaFacil.Application.DTOs.Quotes;

namespace OrcaFacil.Application.Validators.Quotes;

public class QuoteItemInputValidator : AbstractValidator<QuoteItemInputDto>
{
    public QuoteItemInputValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.ServiceId.HasValue || (!string.IsNullOrWhiteSpace(x.Description) && x.UnitPrice.HasValue))
            .WithMessage("Informe um serviço do catálogo (serviceId) ou uma descrição e preço para um item avulso.");
    }
}

public class CreateQuoteValidator : AbstractValidator<CreateQuoteDto>
{
    public CreateQuoteValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ValidUntil.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("A validade não pode ser uma data no passado.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("O orçamento precisa de pelo menos um item.");
        RuleForEach(x => x.Items).SetValidator(new QuoteItemInputValidator());
    }
}

public class UpdateQuoteValidator : AbstractValidator<UpdateQuoteDto>
{
    public UpdateQuoteValidator()
    {
        RuleFor(x => x.ValidUntil.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("A validade não pode ser uma data no passado.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("O orçamento precisa de pelo menos um item.");
        RuleForEach(x => x.Items).SetValidator(new QuoteItemInputValidator());
    }
}

public class UpdateQuoteStatusValidator : AbstractValidator<UpdateQuoteStatusDto>
{
    public UpdateQuoteStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
