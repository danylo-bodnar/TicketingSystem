using Ticketing.Application.Common.Interfaces;
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

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }
    }
}