using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(c => c.Settings)
            .WithOne(s => s.Company)
            .HasForeignKey<CompanySettings>(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
