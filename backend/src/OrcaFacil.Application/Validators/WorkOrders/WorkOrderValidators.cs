using FluentValidation;
using OrcaFacil.Application.DTOs.WorkOrders;

namespace OrcaFacil.Application.Validators.WorkOrders;

public class WorkOrderItemInputValidator : AbstractValidator<WorkOrderItemInputDto>
{
    public WorkOrderItemInputValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.ServiceId.HasValue || (!string.IsNullOrWhiteSpace(x.Description) && x.UnitPrice.HasValue))
            .WithMessage("Informe um serviço do catálogo (serviceId) ou uma descrição e preço para um item avulso.");
    }
}

public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderDto>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("A ordem de serviço precisa de pelo menos um item.");
        RuleForEach(x => x.Items).SetValidator(new WorkOrderItemInputValidator());
    }
}

public class UpdateWorkOrderValidator : AbstractValidator<UpdateWorkOrderDto>
{
    public UpdateWorkOrderValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("A ordem de serviço precisa de pelo menos um item.");
        RuleForEach(x => x.Items).SetValidator(new WorkOrderItemInputValidator());
    }
}

public class UpdateWorkOrderStatusValidator : AbstractValidator<UpdateWorkOrderStatusDto>
{
    public UpdateWorkOrderStatusValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
