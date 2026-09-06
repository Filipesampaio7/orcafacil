using FluentValidation;
using OrcaFacil.Application.DTOs.Auth;

namespace OrcaFacil.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        // Sem MinimumLength aqui de propósito: login é contra uma senha que
        // já existe, então validar "força" da senha não é papel deste form.
    }
}
