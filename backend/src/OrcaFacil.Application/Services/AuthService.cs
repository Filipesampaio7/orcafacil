using OrcaFacil.Application.DTOs.Auth;
using OrcaFacil.Application.Exceptions;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;

namespace OrcaFacil.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<CompanySettings> _companySettingsRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRepository<Company> companyRepository,
        IRepository<CompanySettings> companySettingsRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _companyRepository = companyRepository;
        _companySettingsRepository = companySettingsRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("Já existe uma conta com este e-mail.");
        }

        // As três escritas abaixo (Company, CompanySettings, User) só chegam
        // ao banco juntas, no SaveChangesAsync no final — é o Unit of Work
        // garantindo que não sobra uma Company sem usuário se algo falhar
        // no meio do caminho.
        var company = new Company { Name = request.CompanyName };
        await _companyRepository.AddAsync(company, cancellationToken);

        var settings = new CompanySettings { Company = company, CompanyId = company.Id };
        await _companySettingsRepository.AddAsync(settings, cancellationToken);

        var user = new User
        {
            Company = company,
            CompanyId = company.Id,
            Name = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Owner,
        };
        await _userRepository.AddAsync(user, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user, company);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Mesma mensagem de erro tanto para "e-mail não existe" quanto para
        // "senha errada" — dizer qual dos dois está errado ajudaria um
        // atacante a descobrir quais e-mails têm conta cadastrada.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return BuildAuthResponse(user, user.Company);
    }

    private AuthResponseDto BuildAuthResponse(User user, Company company)
    {
        var token = _tokenService.GenerateToken(user);
        var currentUser = new CurrentUserDto(user.Id, user.Name, user.Email, user.Role.ToString(), company.Id, company.Name);
        return new AuthResponseDto(token.Token, token.ExpiresAt, currentUser);
    }
}
