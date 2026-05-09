using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.EventHandlers;
using Ticketing.Application.Events;
using Ticketing.Application.Outbox;
using Ticketing.Application.Reservations.Services;
using Ticketing.Domain.Events;

namespace Ticketing.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


            services.AddScoped<OutboxProcessorService>();
            services.AddScoped<ReservationExpirationService>();

            services.AddScoped<IEventDispatcher, EventDispatcher>();

            services.AddScoped<IEventHandler<ReservationCreated>, ReservationCreatedHandler>();
            services.AddScoped<IEventHandler<PaymentCompleted>, PaymentCompletedHandler>();
            services.AddScoped<IEventHandler<ReservationConfirmed>, ReservationConfirmedHandler>();

            return services;
        }
    }
}
