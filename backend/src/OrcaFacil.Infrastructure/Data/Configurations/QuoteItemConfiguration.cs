using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.Property(i => i.Description).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);

        // Cascade: um item de orçamento não existe fora do seu orçamento.
        builder.HasOne(i => i.Quote)
            .WithMany(q => q.Items)
            .HasForeignKey(i => i.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // SetNull: se o serviço de catálogo for excluído, o item do orçamento
        // permanece (Description já guarda o nome no momento da emissão).
        builder.HasOne(i => i.Service)
            .WithMany(s => s.QuoteItems)
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
