using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Screenings;
using Ticketing.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Ticketing.Infrastructure.Repositories
{
    public class ScreeningRepository : IScreeningRepository
    {
        private readonly TicketingDbContext _context;

        public ScreeningRepository(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Screening screening, CancellationToken ct = default)
        {
            await _context.Screenings.AddAsync(screening, ct);
        }

        public async Task<List<Screening>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Screenings.ToListAsync(ct);
        }

        public async Task<Screening?> GetByIdAsync(Guid screeningId, CancellationToken ct = default)
        {
            return await _context.Screenings
                .Include(s => s.Seats)
                .FirstOrDefaultAsync(s => s.Id == screeningId, ct);
        }

        public async Task<Screening?> GetSeats(Guid screeningId, CancellationToken ct = default)
        {
            return await _context.Screenings
                .Include(s => s.Seats)
                    .ThenInclude(ss => ss.Seat)
                .FirstOrDefaultAsync(s => s.Id == screeningId, ct);
        }

        public async Task<List<ScreeningSeat>> GetAvailableSeatsAsync(Guid screeningId, CancellationToken ct = default)
        {
            return await _context.ScreeningSeats
                .Include(ss => ss.Seat)
                .Where(ss => ss.ScreeningId == screeningId
                             && ss.Status == ScreeningSeatStatus.Available)
                .ToListAsync(ct);
        }
    }
}