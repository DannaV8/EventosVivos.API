using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        mb.Entity<Venue>().HasData(
            new Venue { Id = 1, Name = "Auditorio Central", Capacity = 200, City = "Bogotá" },
            new Venue { Id = 2, Name = "Sala Norte",        Capacity = 50,  City = "Bogotá" },
            new Venue { Id = 3, Name = "Arena Sur",         Capacity = 500, City = "Medellín" }
        );
    }
}
