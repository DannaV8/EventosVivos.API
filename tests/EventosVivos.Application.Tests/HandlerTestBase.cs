using EventosVivos.Domain.Services;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EventosVivos.Application.Tests;
public abstract class HandlerTestBase : IDisposable
{
    protected AppDbContext Db { get; }
    protected EventRepository Events { get; }
    protected ReservationRepository Reservations { get; }
    protected VenueRepository Venues { get; }
    protected ReservationValidationService Validation { get; } = new();

    protected HandlerTestBase()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Db = new AppDbContext(opts);
        Db.Database.EnsureCreated();

        Events = new EventRepository(Db, NullLogger<EventRepository>.Instance);
        Reservations = new ReservationRepository(Db, NullLogger<ReservationRepository>.Instance);
        Venues = new VenueRepository(Db);
    }

    public void Dispose() => Db.Dispose();
}
