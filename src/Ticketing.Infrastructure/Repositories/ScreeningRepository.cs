using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Screenings;
using Ticketing.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Ticketing.Contracts.DTOs;

namespace Ticketing.Infrastructure.Repositories
{
    public class ScreeningRepository : IScreeningRepository
    {
        private readonly TicketingDbContext _context;

        public ScreeningRepository(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Screening screening)
        {
            await _context.Screenings.AddAsync(screening);
        }

        public async Task<List<Screening>> GetAllAsync()
        {
            return await _context.Screenings.ToListAsync();
        }

        public async Task<Screening?> GetByIdAsync(Guid screeningId)
        {
            return await _context.Screenings
                .Include(s => s.Seats)
                .FirstOrDefaultAsync(s => s.Id == screeningId);
        }

        public async Task<List<SeatDto>> GetAvailableSeatsAsync(Guid screeningId)
        {
            return await _context.ScreeningSeats
                .Where(ss => ss.ScreeningId == screeningId
                             && ss.Status == ScreeningSeatStatus.Available)
                .Join(
                    _context.Seats,
                    ss => ss.SeatId,
                    s => s.Id,
                    (ss, s) => new SeatDto
                    {
                        SeatId = s.Id,
                        Row = s.Row,
                        Column = s.Column,
                        Status = ss.Status
                    })
                .ToListAsync();
        }
    }
}