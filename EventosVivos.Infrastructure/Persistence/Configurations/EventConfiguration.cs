using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> b)
    {
        b.HasKey(e => e.Id);

        b.Property(e => e.Title).HasMaxLength(100).IsRequired();
        b.Property(e => e.Description).HasMaxLength(500).IsRequired();
        b.Property(e => e.TicketPrice).HasColumnType("decimal(18,2)");
        b.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        b.Property<bool>("_cancelled").HasColumnName("IsCancelled").HasDefaultValue(false);
        b.Ignore(e => e.Status);

        b.Property<byte[]>("RowVersion").IsRowVersion().IsConcurrencyToken();

        b.HasOne(e => e.Venue).WithMany().HasForeignKey(e => e.VenueId);

        b.HasIndex(e => new { e.VenueId, e.StartDateTime, e.EndDateTime });
    }
}
