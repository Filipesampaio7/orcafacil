using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.Property(q => q.Subtotal).HasPrecision(18, 2);
        builder.Property(q => q.DiscountAmount).HasPrecision(18, 2);
        builder.Property(q => q.Total).HasPrecision(18, 2);
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.TechnicalObservations).HasMaxLength(1000);

        // Garante, no nível do banco, que dois orçamentos da mesma empresa
        // nunca dividam o mesmo número visível ao cliente (#0001).
        builder.HasIndex(q => new { q.CompanyId, q.Number }).IsUnique();

        // Acelera o filtro "orçamentos pendentes/aprovados" do Dashboard (FASE 9).
        builder.HasIndex(q => new { q.CompanyId, q.Status });

        builder.HasOne(q => q.Company)
            .WithMany(c => c.Quotes)
            .HasForeignKey(q => q.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict (em vez de Cascade): não deixamos excluir um cliente que já
        // tem orçamentos — é histórico do negócio, não deve sumir por acidente.
        builder.HasOne(q => q.Customer)
            .WithMany(c => c.Quotes)
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.WorkOrder)
            .WithOne(w => w.Quote)
            .HasForeignKey<WorkOrder>(w => w.QuoteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
