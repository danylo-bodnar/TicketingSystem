using MediatR;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Payments.Commands;
using Ticketing.Domain.Payments;

namespace Ticketing.Application.Payments.Handlers
{
    public class FailPaymentHandler(
        IPaymentRepository _paymentRepository,
        IUnitOfWork _unitOfWork,
        ILogger<FailPaymentHandler> _logger) : IRequestHandler<FailPaymentCommand>
    {
        public async Task Handle(FailPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetById(request.PaymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                throw new InvalidOperationException($"Payment with ID {request.PaymentId} not found.");
            }

            payment.Fail();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment {PaymentId} failed for reservation {ReservationId}",
                payment.Id, payment.ReservationId);
        }
    }
}
