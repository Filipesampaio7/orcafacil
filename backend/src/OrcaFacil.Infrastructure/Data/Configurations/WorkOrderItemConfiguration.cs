using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class WorkOrderItemConfiguration : IEntityTypeConfiguration<WorkOrderItem>
{
    public void Configure(EntityTypeBuilder<WorkOrderItem> builder)
    {
        builder.Property(i => i.Description).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Quantity).HasPrecision(18, 2);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);

        builder.HasOne(i => i.WorkOrder)
            .WithMany(w => w.Items)
            .HasForeignKey(i => i.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Service)
            .WithMany(s => s.WorkOrderItems)
            .HasForeignKey(i => i.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
