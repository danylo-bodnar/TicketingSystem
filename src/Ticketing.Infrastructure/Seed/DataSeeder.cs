using Ticketing.Domain.Halls;
using Ticketing.Domain.Seats;
using Ticketing.Domain.Screenings;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.Infrastructure.Seed
{
    public static class DataSeeder
    {
        public static readonly Guid Hall1Id = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000001");
        public static readonly Guid Hall2Id = Guid.Parse("a2a2a2a2-0000-0000-0000-000000000002");
        public static readonly Guid Screening1Id = Guid.Parse("b1b1b1b1-0000-0000-0000-000000000001");
        public static readonly Guid Screening2Id = Guid.Parse("b2b2b2b2-0000-0000-0000-000000000002");

        public static void Seed(TicketingDbContext context)
        {
            if (!context.Halls.Any())
            {
                // --- Create Halls with Seats ---
                var hall1Id = Hall1Id;
                var hall1Seats = new List<Seat>();
                for (int row = 1; row <= 5; row++)
                {
                    for (int col = 1; col <= 10; col++)
                    {
                        hall1Seats.Add(new Seat(Guid.NewGuid(), hall1Id, row.ToString(), col));
                    }
                }
                var hall1 = new Hall(hall1Id, "IMAX", hall1Seats);
                context.Halls.Add(hall1);

                var hall2Id = Hall2Id;
                var hall2Seats = new List<Seat>();
                for (int row = 1; row <= 4; row++)
                {
                    for (int col = 1; col <= 8; col++)
                    {
                        hall2Seats.Add(new Seat(Guid.NewGuid(), hall2Id, row.ToString(), col));
                    }
                }
                var hall2 = new Hall(hall2Id, "Standard Hall", hall2Seats);
                context.Halls.Add(hall2);

                context.SaveChanges();

                // --- Create Screenings with ScreeningSeats ---
                var screening1Seats = hall1Seats.Select(s =>
                    new ScreeningSeat(Guid.NewGuid(), Screening1Id, s.Id)).ToList();
                var screening1 = new Screening(Screening1Id, Guid.NewGuid(), hall1Id, DateTime.UtcNow.AddHours(2), screening1Seats);
                context.Screenings.Add(screening1);

                var screening2Seats = hall2Seats.Select(s =>
                    new ScreeningSeat(Guid.NewGuid(), Screening2Id, s.Id)).ToList();
                var screening2 = new Screening(Screening2Id, Guid.NewGuid(), hall2Id, DateTime.UtcNow.AddHours(3), screening2Seats);
                context.Screenings.Add(screening2);

                context.SaveChanges();
            }
        }
    }
}