using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrcaFacil.Domain.Entities;
using OrcaFacil.Domain.Enums;
using OrcaFacil.Infrastructure.Data;
using OrcaFacil.Infrastructure.Repositories;
using Xunit;

namespace OrcaFacil.Tests.Dashboard;

/// <summary>
/// Único teste do projeto que usa um banco de verdade (SQLite em memória) —
/// justificado porque isolamento entre empresas é exatamente o tipo de
/// garantia que um teste puro (sem banco) não prova: a cláusula WHERE do SQL
/// gerado é o que realmente importa aqui, não só a lógica em C#.
///
/// EnsureCreated() (em vez de aplicar as migrations) é suficiente para um
/// teste: cria o schema a partir do modelo do EF Core diretamente. Mais
/// rápido, e não depende do assembly de Migrations.
/// </summary>
public class DashboardRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly DashboardRepository _repository;

    public DashboardRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new DashboardRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetSummaryAsync_NaoDeveMisturarDadosDeOutraEmpresa()
    {
        var companyA = new Company { Name = "Empresa A" };
        var companyB = new Company { Name = "Empresa B" };
        var customerA = new Customer { Company = companyA, CompanyId = companyA.Id, Name = "Cliente A" };
        var customerB = new Customer { Company = companyB, CompanyId = companyB.Id, Name = "Cliente B" };

        _context.AddRange(companyA, companyB, customerA, customerB);

        _context.Quotes.Add(new Quote
        {
            Company = companyA,
            CompanyId = companyA.Id,
            Customer = customerA,
            CustomerId = customerA.Id,
            Number = 1,
            Status = QuoteStatus.Approved,
            ValidUntil = DateTime.UtcNow.AddDays(7),
            Total = 100m,
        });

        // Orçamento da Empresa B, com valor bem maior — se o isolamento
        // falhar, esse valor vai "vazar" para o resultado da Empresa A.
        _context.Quotes.Add(new Quote
        {
            Company = companyB,
            CompanyId = companyB.Id,
            Customer = customerB,
            CustomerId = customerB.Id,
            Number = 1,
            Status = QuoteStatus.Approved,
            ValidUntil = DateTime.UtcNow.AddDays(7),
            Total = 9999m,
        });

        await _context.SaveChangesAsync();

        var summaryA = await _repository.GetSummaryAsync(companyA.Id);
        var summaryB = await _repository.GetSummaryAsync(companyB.Id);

        Assert.Equal(1, summaryA.TotalQuotes);
        Assert.Equal(100m, summaryA.EstimatedRevenue);

        Assert.Equal(1, summaryB.TotalQuotes);
        Assert.Equal(9999m, summaryB.EstimatedRevenue);
    }

    [Fact]
    public async Task GetQuoteStatusCountsAsync_NaoDeveContarOrcamentosDeOutraEmpresa()
    {
        var companyA = new Company { Name = "Empresa A" };
        var companyB = new Company { Name = "Empresa B" };
        var customerA = new Customer { Company = companyA, CompanyId = companyA.Id, Name = "Cliente A" };
        var customerB = new Customer { Company = companyB, CompanyId = companyB.Id, Name = "Cliente B" };

        _context.AddRange(companyA, companyB, customerA, customerB);

        // Empresa A: 1 aprovado. Empresa B: 3 aprovados. Se o filtro falhar,
        // a contagem da Empresa A viria 4, não 1.
        _context.Quotes.Add(new Quote
        {
            Company = companyA, CompanyId = companyA.Id, Customer = customerA, CustomerId = customerA.Id,
            Number = 1, Status = QuoteStatus.Approved, ValidUntil = DateTime.UtcNow.AddDays(7),
        });

        for (var i = 1; i <= 3; i++)
        {
            _context.Quotes.Add(new Quote
            {
                Company = companyB, CompanyId = companyB.Id, Customer = customerB, CustomerId = customerB.Id,
                Number = i, Status = QuoteStatus.Approved, ValidUntil = DateTime.UtcNow.AddDays(7),
            });
        }

        await _context.SaveChangesAsync();

        var countsA = await _repository.GetQuoteStatusCountsAsync(companyA.Id, sinceUtc: null);

        Assert.Equal(1, countsA.Approved);
        Assert.Equal(1, countsA.Total);
    }

    [Fact]
    public async Task GetServiceRankingAsync_NaoDeveIncluirServicoDeOutraEmpresa()
    {
        var companyA = new Company { Name = "Empresa A" };
        var companyB = new Company { Name = "Empresa B" };
        var customerA = new Customer { Company = companyA, CompanyId = companyA.Id, Name = "Cliente A" };
        var customerB = new Customer { Company = companyB, CompanyId = companyB.Id, Name = "Cliente B" };
        var serviceA = new Service { Company = companyA, CompanyId = companyA.Id, Name = "Troca de óleo", DefaultPrice = 100m };
        var serviceB = new Service { Company = companyB, CompanyId = companyB.Id, Name = "Alinhamento", DefaultPrice = 200m };

        _context.AddRange(companyA, companyB, customerA, customerB, serviceA, serviceB);

        var workOrderA = new WorkOrder
        {
            Company = companyA, CompanyId = companyA.Id, Customer = customerA, CustomerId = customerA.Id,
            Status = WorkOrderStatus.Completed,
        };
        var itemA = new WorkOrderItem { WorkOrder = workOrderA, Service = serviceA, ServiceId = serviceA.Id, Quantity = 1, UnitPrice = 100m, LineTotal = 100m };
        workOrderA.Items.Add(itemA);

        var workOrderB = new WorkOrder
        {
            Company = companyB, CompanyId = companyB.Id, Customer = customerB, CustomerId = customerB.Id,
            Status = WorkOrderStatus.Completed,
        };
        var itemB = new WorkOrderItem { WorkOrder = workOrderB, Service = serviceB, ServiceId = serviceB.Id, Quantity = 1, UnitPrice = 200m, LineTotal = 200m };
        workOrderB.Items.Add(itemB);

        _context.AddRange(workOrderA, workOrderB);
        await _context.SaveChangesAsync();

        var rankingA = await _repository.GetServiceRankingAsync(companyA.Id, take: 10);

        var serviceNames = rankingA.Select(r => r.ServiceName).ToList();
        Assert.Contains("Troca de óleo", serviceNames);
        Assert.DoesNotContain("Alinhamento", serviceNames);
    }
}
