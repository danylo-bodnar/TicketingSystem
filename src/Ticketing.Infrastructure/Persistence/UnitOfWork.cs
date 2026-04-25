using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Domain.Common.Exceptions;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Infrastructure.Persistence.Extenisons;

namespace Ticketing.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TicketingDbContext _context;

        public UnitOfWork(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException("Data was modified by another user.");
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new DuplicateEntityException();
            }
        }
    }
}
