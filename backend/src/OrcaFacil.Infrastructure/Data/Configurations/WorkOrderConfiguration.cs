using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.TechnicalObservations).HasMaxLength(1000);

        // Acelera o quadro de ordens de serviço por status (FASE 8/9).
        builder.HasIndex(w => new { w.CompanyId, w.Status });

        builder.HasOne(w => w.Company)
            .WithMany(c => c.WorkOrders)
            .HasForeignKey(w => w.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Customer)
            .WithMany(c => c.WorkOrders)
            .HasForeignKey(w => w.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull: remover um usuário não pode apagar a ordem de serviço,
        // só deixá-la sem responsável atribuído.
        builder.HasOne(w => w.AssignedUser)
            .WithMany(u => u.AssignedWorkOrders)
            .HasForeignKey(w => w.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // A ponta QuoteId->Quote já foi configurada em QuoteConfiguration
        // (relação 1:1 definida a partir de Quote.WorkOrder).
    }
}
