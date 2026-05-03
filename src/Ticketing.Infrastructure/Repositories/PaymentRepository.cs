using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Payments;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly TicketingDbContext _context;

        public PaymentRepository(TicketingDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment, CancellationToken ct = default)
        {
            await _context.Payments.AddAsync(payment, ct);
        }

        public async Task<Payment?> GetById(Guid paymentId, CancellationToken ct = default)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        }
    }
}
