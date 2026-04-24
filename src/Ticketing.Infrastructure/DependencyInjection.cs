using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Infrastructure.Outbox;
using Ticketing.Infrastructure.Persistence;
using Ticketing.Infrastructure.Repositories;

namespace Ticketing.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
                this IServiceCollection services,
                IConfiguration configuration)
        {
            services.AddDbContext<TicketingDbContext>(options =>
                     options.UseNpgsql(
                         configuration.GetConnectionString("DefaultConnection")
                     ));

            services.AddScoped<IHallRepository, HallRepository>();
            services.AddScoped<IScreeningRepository, ScreeningRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddSingleton<IConnectionMultiplexer>(
                      ConnectionMultiplexer.Connect("localhost:6379"));
            services.AddScoped<ISeatLockService, RedisSeatLockService>();

            return services;
        }
    }
}
