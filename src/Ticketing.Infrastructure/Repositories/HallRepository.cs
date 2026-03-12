using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Halls;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.Infrastructure.Repositories
{
    public class HallRepository : IHallRepository
    {
        private readonly TicketingDbContext _context;

        public HallRepository(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Hall hall)
        {
            await _context.Halls.AddAsync(hall);
        }

        public async Task<Hall?> GetByIdAsync(Guid hallId)
        {
            return await _context.Halls.Include(h => h.Seats).FirstOrDefaultAsync(h => h.Id == hallId);
        }
    }
}