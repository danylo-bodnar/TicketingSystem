using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Halls;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;
using Ticketing.Infrastructure.Outbox;

namespace Ticketing.Infrastructure.Contexts
{
    public class TicketingDbContext : DbContext
    {
        public TicketingDbContext(DbContextOptions<TicketingDbContext> options) : base(options)
        {
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
                .Property(ss => ss.Version)
                .IsRowVersion(); ;

            modelBuilder.Entity<Payment>(p =>
            {
                p.OwnsOne(x => x.Amount);
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
                    OccurredAt = DateTime.UtcNow
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