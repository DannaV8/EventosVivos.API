using EventosVivos.Application.Common.Interfaces;
using EventosVivos.Infrastructure.Auth;
using EventosVivos.Infrastructure.Locks;
using EventosVivos.Infrastructure.Persistence;
using EventosVivos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Default")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<JwtService>();
        services.AddSingleton<ITokenGenerator>(sp => sp.GetRequiredService<JwtService>());
        services.AddSingleton<IEventLockProvider, EventLockProvider>();

        return services;
    }
}
