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

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
        }

        public async Task<Reservation?> GetByIdAsync(Guid reservationId)
        {
            return await _context.Reservations.FirstOrDefaultAsync(r => r.Id == reservationId);
        }
    }
}