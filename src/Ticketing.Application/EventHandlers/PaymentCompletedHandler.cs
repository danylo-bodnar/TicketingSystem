using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

namespace Ticketing.Application.EventHandlers
{
    public class PaymentCompletedHandler : IEventHandler<PaymentCompleted>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentCompletedHandler> _logger;

        public PaymentCompletedHandler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork, ILogger<PaymentCompletedHandler> logger)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(PaymentCompleted @event, CancellationToken ct)
        {
            _logger.LogInformation("Handling PaymentCompleted for reservation {ReservationId}",
                        @event.ReservationId);

            var reservation = await _reservationRepository.GetByIdAsync(@event.ReservationId, ct);
            if (reservation == null)
                throw new InvalidOperationException("Reservation not found");

            reservation.Confirm();

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Reservation {ReservationId} confirmed", @event.ReservationId);

        }
    }
}