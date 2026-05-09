using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;

namespace Ticketing.Application.Reservations.Services
{
    public class ReservationExpirationService
    {
        private readonly IReservationRepository _reservations;
        private readonly IScreeningRepository _screenings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReservationExpirationService> _logger;

        public ReservationExpirationService(
            IReservationRepository reservations,
            IScreeningRepository screenings,
            IUnitOfWork unitOfWork,
            ILogger<ReservationExpirationService> logger)
        {
            _reservations = reservations;
            _screenings = screenings;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessOnce(CancellationToken ct)
        {
            var expiredReservations = await _reservations.GetExpiredAsync(ct);

            foreach (var reservation in expiredReservations)
            {
                try
                {
                    reservation.Expire();

                    var screening = await _screenings.GetByIdAsync(reservation.ScreeningId, ct);
                    if (screening == null)
                    {
                        _logger.LogWarning("Screening {ScreeningId} not found for reservation {ReservationId}",
                            reservation.ScreeningId, reservation.Id);
                        continue;
                    }

                    foreach (var seatId in reservation.SeatIds)
                        screening.GetSeat(seatId).Release();

                    await _unitOfWork.SaveChangesAsync(ct);

                    _logger.LogInformation("Reservation {ReservationId} expired, {SeatCount} seats released",
                        reservation.Id, reservation.SeatIds.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to expire reservation {ReservationId}", reservation.Id);
                }
            }
        }
    }
}