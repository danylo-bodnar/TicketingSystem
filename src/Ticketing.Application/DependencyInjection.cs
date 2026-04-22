using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

namespace Ticketing.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.AddScoped<IEventDispatcher, EventDispatcher>();

            services.AddScoped<IEventHandler<ReservationCreated>, ReservationCreatedHandler>();

            return services;
        }
    }
}