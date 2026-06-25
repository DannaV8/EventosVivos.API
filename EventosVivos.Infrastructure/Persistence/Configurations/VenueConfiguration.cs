using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> b)
    {
        b.HasKey(v => v.Id);
        b.Property(v => v.Name).HasMaxLength(100).IsRequired();
        b.Property(v => v.City).HasMaxLength(100).IsRequired();
    }
}
