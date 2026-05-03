using MediatR;
using Ticketing.Application.Payments.Commands;
using Ticketing.Domain.Payments;

namespace Ticketing.Application.Payments.Handlers
{
    public class CompletePaymentHandler : IRequestHandler<CompletePaymentCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompletePaymentHandler(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _paymentRepository.GetById(request.PaymentId);

            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            payment.Complete();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
