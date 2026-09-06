using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Category).HasMaxLength(100);

        // HasPrecision evita ambiguidade no mapeamento de decimal entre
        // providers — sem isso, alguns bancos podem gerar um tipo numérico
        // sem escala definida ou emitir warning. 18 dígitos totais, 2 casas
        // decimais (padrão para valores monetários em BRL).
        builder.Property(s => s.DefaultPrice).HasPrecision(18, 2);

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(s => s.Company)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
