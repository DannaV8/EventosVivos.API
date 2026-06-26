using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);

        b.Property(u => u.Email).HasMaxLength(320).IsRequired();
        b.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
        b.Property(u => u.Role).HasMaxLength(10).IsRequired();

        // Unique email — login key.
        b.HasIndex(u => u.Email).IsUnique();
    }
}
