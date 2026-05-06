using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common;
using Ticketing.Application.Outbox;
using Ticketing.Domain.Halls;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;

namespace Ticketing.Infrastructure.Contexts
{
    public class TicketingDbContext : DbContext
    {
        private readonly CorrelationContext _correlationContext;

        public TicketingDbContext(DbContextOptions<TicketingDbContext> options, CorrelationContext correlationContext) : base(options)
        {
            _correlationContext = correlationContext;
        }

        public DbSet<Hall> Halls => Set<Hall>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Screening> Screenings => Set<Screening>();
        public DbSet<ScreeningSeat> ScreeningSeats => Set<ScreeningSeat>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Payment> Payments => Set<Payment>();

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Hall>()
                .HasMany(h => h.Seats)
                .WithOne()
                .HasForeignKey(s => s.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Screening>()
                .HasMany(s => s.Seats)
                .WithOne()
                .HasForeignKey(ss => ss.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ScreeningSeat>()
                .HasOne(ss => ss.Seat)
                .WithMany()
                .HasForeignKey(ss => ss.SeatId);

            modelBuilder.Entity<Payment>(p =>
            {
                p.OwnsOne(x => x.Amount);

                p.HasIndex(x => x.ReservationId)
                 .IsUnique();
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var domainEvents = ChangeTracker
                .Entries<AggregateRoot>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                OutboxMessages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent),
                    OccurredAt = DateTime.UtcNow,
                    CorrelationId = _correlationContext.CorrelationId
                });
            }

            var result = await base.SaveChangesAsync(ct);

            foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            {
                entry.Entity.ClearDomainEvents();
            }

            return result;
        }
    }
}
