using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

namespace Ticketing.Application.EventHandlers
{
    public class ReservationConfirmedHandler : IEventHandler<ReservationConfirmed>
    {
        private readonly IScreeningRepository _screeningRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservationConfirmedHandler> _logger;

        public ReservationConfirmedHandler(
            IScreeningRepository screeningRepository,
            IUnitOfWork unitOfWork,
            ILogger<ReservationConfirmedHandler> logger)
        {
            _screeningRepository = screeningRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(ReservationConfirmed @event, CancellationToken ct)
        {
            _logger.LogInformation("Marking seats as sold for reservation {ReservationId}, screening {ScreeningId}",
                @event.ReservationId, @event.ScreeningId);

            var screening = await _screeningRepository.GetByIdAsync(@event.ScreeningId, ct);
            if (screening == null)
                throw new InvalidOperationException("Screening not found");

            foreach (var seatId in @event.SeatIds)
                screening.GetSeat(seatId).MarkAsSold();

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("{SeatCount} seats marked as sold for reservation {ReservationId}",
                @event.SeatIds.Count, @event.ReservationId);
        }
    }
}