using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

namespace Ticketing.Application.EventHandlers
{
    public class PaymentCompletedHandler : IEventHandler<PaymentCompleted>
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentCompletedHandler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork)
        {
            _reservationRepository = reservationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task HandleAsync(PaymentCompleted @event, CancellationToken ct)
        {
            var reservation = await _reservationRepository.GetByIdAsync(@event.ReservationId, ct);
            if (reservation == null)
                throw new InvalidOperationException("Reservation not found");

            reservation.Confirm();

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}