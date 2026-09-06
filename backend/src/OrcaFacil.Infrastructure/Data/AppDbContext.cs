using Microsoft.EntityFrameworkCore;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data;

/// <summary>
/// Ponte entre as entidades do Domain e o PostgreSQL. Cada IEntityTypeConfiguration
/// (pasta Configurations/) descreve o mapeamento de uma entidade — isso evita
/// um único OnModelCreating gigante com tudo misturado.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderItem> WorkOrderItems => Set<WorkOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Varre o assembly da Infrastructure e aplica toda classe que
        // implementar IEntityTypeConfiguration<T> — não precisamos listar
        // cada configuração manualmente aqui.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Preenche UpdatedAt automaticamente em qualquer entidade modificada,
    /// para nenhum Service precisar lembrar de fazer isso manualmente.
    /// </summary>
    private void UpdateTimestamps()
    {
        var modifiedEntries = ChangeTracker.Entries<Domain.Common.BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in modifiedEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
