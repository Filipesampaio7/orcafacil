namespace OrcaFacil.Application.Exceptions;

/// <summary>
/// Usada tanto para "id não existe" quanto para "id existe, mas é de outra
/// empresa" — de propósito a mesma resposta para os dois casos. Se
/// devolvêssemos algo diferente (ex.: 403 Forbidden) quando o registro é de
/// outra empresa, isso vazaria a informação de que aquele id existe no
/// sistema, mesmo sem acesso a ele.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
