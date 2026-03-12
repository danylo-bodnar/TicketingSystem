using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Halls;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;

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
        }
    }
}