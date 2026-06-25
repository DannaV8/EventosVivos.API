using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> b)
    {
        b.HasKey(r => r.Id);

        b.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(r => r.ReservationCode).HasMaxLength(9);
        b.Property(r => r.BuyerEmail).HasMaxLength(320).IsRequired();
        b.Property(r => r.BuyerName).HasMaxLength(200).IsRequired();

        b.HasOne<Event>().WithMany().HasForeignKey(r => r.EventId);
        b.HasOne<User>().WithMany().HasForeignKey(r => r.UserId);

        b.HasIndex(r => new { r.EventId, r.Status });
        b.HasIndex(r => r.UserId);

        b.HasIndex(r => r.ReservationCode)
            .IsUnique()
            .HasFilter("\"ReservationCode\" IS NOT NULL");
    }
}
