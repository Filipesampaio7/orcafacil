using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrcaFacil.Domain.Entities;

namespace OrcaFacil.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired();

        // Simplificação de MVP: e-mail único em todo o sistema (um usuário
        // pertence a uma única empresa). Se no futuro uma pessoa precisar
        // acessar mais de uma empresa com o mesmo e-mail, isso exige uma
        // tabela de associação User<->Company em vez de FK direta.
        builder.HasIndex(u => u.Email).IsUnique();

        // Enum salvo como string: um pouco mais de espaço em disco, mas
        // quem for investigar dados direto no banco (ex.: suporte, debug)
        // lê "Owner" em vez de adivinhar o que o inteiro 0 significa.
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
