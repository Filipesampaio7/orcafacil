namespace OrcaFacil.Application.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("E-mail ou senha inválidos.")
    {
    }
}
