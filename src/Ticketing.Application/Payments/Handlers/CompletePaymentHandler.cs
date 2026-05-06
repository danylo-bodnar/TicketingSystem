using MediatR;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Payments.Commands;
using Ticketing.Domain.Payments;

namespace Ticketing.Application.Payments.Handlers
{
    public class CompletePaymentHandler : IRequestHandler<CompletePaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CompletePaymentHandler> _logger;

        public CompletePaymentHandler(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            ILogger<CompletePaymentHandler> logger)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetById(request.PaymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", request.PaymentId);
                throw new InvalidOperationException("Payment not found");
            }

            payment.Complete();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Payment {PaymentId} completed for reservation {ReservationId}",
                payment.Id, payment.ReservationId);
        }
    }
}
