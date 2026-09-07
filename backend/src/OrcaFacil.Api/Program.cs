using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrcaFacil.Api.Middleware;
using OrcaFacil.Application.DTOs.Customers;
using OrcaFacil.Application.DTOs.Quotes;
using OrcaFacil.Application.DTOs.Services;
using OrcaFacil.Application.Interfaces;
using OrcaFacil.Application.Services;
using OrcaFacil.Application.Validators.Auth;
using OrcaFacil.Application.Validators.Customers;
using OrcaFacil.Application.Validators.Quotes;
using OrcaFacil.Application.Validators.Services;
using FluentValidation;
using OrcaFacil.Application.DTOs.Auth;
using OrcaFacil.Infrastructure.Authentication;
using OrcaFacil.Infrastructure.Data;
using OrcaFacil.Infrastructure.Pdf;
using OrcaFacil.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// --- Registro de serviços (Dependency Injection) -----------------------

// AuthorizeFilter global: toda action de todo controller passa a exigir
// autenticação por padrão. Endpoints públicos (login, cadastro, health)
// precisam do atributo [AllowAnonymous] explicitamente — "seguro por
// padrão" em vez de "esquecer de proteger uma rota nova".
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// A connection string real vem de variável de ambiente / User Secrets, nunca
// commitada. Veja README.md e .env.example na raiz do backend.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository e UnitOfWork genéricos, disponíveis para qualquer entidade via
// injeção de dependência.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<ICompanySettingsRepository, CompanySettingsRepository>();

// Autenticação: hashing de senha, geração/leitura de JWT.
builder.Services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IQuotePdfGenerator, QuestPdfQuoteGenerator>();

// Validadores do FluentValidation, injetáveis diretamente nos controllers.
builder.Services.AddScoped<IValidator<RegisterRequestDto>, RegisterRequestValidator>();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();
builder.Services.AddScoped<IValidator<CreateServiceDto>, CreateServiceValidator>();
builder.Services.AddScoped<IValidator<UpdateServiceDto>, UpdateServiceValidator>();
builder.Services.AddScoped<IValidator<CreateQuoteDto>, CreateQuoteValidator>();
builder.Services.AddScoped<IValidator<UpdateQuoteDto>, UpdateQuoteValidator>();
builder.Services.AddScoped<IValidator<UpdateQuoteStatusDto>, UpdateQuoteStatusValidator>();

var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // MapInboundClaims = false: sem isso, o ASP.NET Core silenciosamente
        // renomeia claims padrão (ex.: "sub" vira uma URI enorme do schema
        // XML antigo). Desligar mantém os nomes exatamente como o
        // TokenService os gerou, então CurrentUserService.FindFirst("sub")
        // realmente encontra o valor.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();

// CORS liberado apenas para a origem do frontend, configurável por ambiente
// (nunca "AllowAnyOrigin" em produção).
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:3000";
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- Pipeline HTTP -------------------------------------------------------

// Primeiro middleware do pipeline: captura qualquer exceção que escape de
// um controller e traduz para uma resposta HTTP consistente.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
