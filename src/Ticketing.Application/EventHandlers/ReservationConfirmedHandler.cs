using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

namespace Ticketing.Application.EventHandlers
{
    public class ReservationConfirmedHandler : IEventHandler<ReservationConfirmed>
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReservationConfirmedHandler(IScreeningRepository screeningRepository, IUnitOfWork unitOfWork)
        {
            _screeningRepository = screeningRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task HandleAsync(ReservationConfirmed @event, CancellationToken ct)
        {
            var screening = await _screeningRepository.GetByIdAsync(@event.ScreeningId, ct);
            if (screening == null)
            {
                throw new InvalidOperationException("Screening not found");
            }

            foreach (var seatId in @event.SeatIds)
            {
                screening.GetSeat(seatId).MarkAsSold();
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}