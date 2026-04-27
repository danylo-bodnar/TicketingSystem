using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Reservations;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly TicketingDbContext _context;

        public ReservationRepository(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
        {
            await _context.Reservations.AddAsync(reservation, ct);
        }

        public async Task<Reservation?> GetByIdAsync(Guid reservationId, CancellationToken ct = default)
        {
            return await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId, ct);
        }
    }
}