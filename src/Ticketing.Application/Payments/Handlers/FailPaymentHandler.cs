using MediatR;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Payments.Commands;

namespace Ticketing.Application.Payments.Handlers
{
    public class FailPaymentHandler(IPaymentRepository _paymentRepository, IUnitOfWork _unitOfWork) : IRequestHandler<FailPaymentCommand>
    {
        public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetById(request.PaymentId);
            if (payment == null)
            {
                throw new InvalidOperationException($"Payment with ID {request.PaymentId} not found.");
            }

            payment.Fail();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}