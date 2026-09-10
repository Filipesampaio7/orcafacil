using OrcaFacil.Application.DTOs.WorkOrders;
using OrcaFacil.Application.Validators.WorkOrders;
using Xunit;

namespace OrcaFacil.Tests.WorkOrders;

public class WorkOrderItemInputValidatorTests
{
    private readonly WorkOrderItemInputValidator _validator = new();

    [Fact]
    public void Validate_ComServiceId_DeveSerValido()
    {
        var item = new WorkOrderItemInputDto(Guid.NewGuid(), null, 1, null);

        Assert.True(_validator.Validate(item).IsValid);
    }

    [Fact]
    public void Validate_SemServiceIdEDescricaoOuPreco_DeveGerarErro()
    {
        var item = new WorkOrderItemInputDto(null, null, 1, null);

        Assert.False(_validator.Validate(item).IsValid);
    }
}

public class CreateWorkOrderValidatorTests
{
    private readonly CreateWorkOrderValidator _validator = new();

    [Fact]
    public void Validate_SemItens_DeveGerarErro()
    {
        var request = new CreateWorkOrderDto(Guid.NewGuid(), null, null, null, null, new List<WorkOrderItemInputDto>());

        Assert.False(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComDadosValidos_DeveSerValido()
    {
        var items = new List<WorkOrderItemInputDto> { new(Guid.NewGuid(), null, 1, null) };
        var request = new CreateWorkOrderDto(Guid.NewGuid(), null, DateTime.UtcNow.AddDays(1), "Observação", null, items);

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComRessalvaTecnicaPreenchida_DeveSerValido()
    {
        var items = new List<WorkOrderItemInputDto> { new(Guid.NewGuid(), null, 1, null) };
        var request = new CreateWorkOrderDto(
            Guid.NewGuid(), null, DateTime.UtcNow.AddDays(1), null,
            "Peça X apresenta desgaste, cliente optou por não autorizar a troca no momento.", items);

        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Validate_ComRessalvaTecnicaMuitoLonga_DeveGerarErro()
    {
        var items = new List<WorkOrderItemInputDto> { new(Guid.NewGuid(), null, 1, null) };
        var tooLong = new string('x', 1001);
        var request = new CreateWorkOrderDto(Guid.NewGuid(), null, DateTime.UtcNow.AddDays(1), null, tooLong, items);

        Assert.False(_validator.Validate(request).IsValid);
    }
}
