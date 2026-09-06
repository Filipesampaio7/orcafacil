using OrcaFacil.Application.DTOs.Auth;
using OrcaFacil.Application.Validators.Auth;
using Xunit;

namespace OrcaFacil.Tests.Authentication;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_ComDadosValidos_NaoDeveGerarErros()
    {
        var request = new RegisterRequestDto("Minha Empresa LTDA", "Ana Souza", "ana@empresa.com", "SenhaForte123!");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Ana Souza", "ana@empresa.com", "SenhaForte123!")] // sem nome de empresa
    [InlineData("Minha Empresa", "Ana Souza", "email-invalido", "SenhaForte123!")] // e-mail inválido
    [InlineData("Minha Empresa", "Ana Souza", "ana@empresa.com", "123")] // senha curta
    public void Validate_ComDadosInvalidos_DeveGerarErro(
        string companyName, string userName, string email, string password)
    {
        var request = new RegisterRequestDto(companyName, userName, email, password);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Validate_ComEmailESenhaPreenchidos_NaoDeveGerarErros()
    {
        var result = _validator.Validate(new LoginRequestDto("ana@empresa.com", "qualquer-coisa"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ComEmailInvalido_DeveGerarErro()
    {
        var result = _validator.Validate(new LoginRequestDto("nao-e-email", "qualquer-coisa"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ComSenhaVazia_DeveGerarErro()
    {
        var result = _validator.Validate(new LoginRequestDto("ana@empresa.com", ""));

        Assert.False(result.IsValid);
    }
}
