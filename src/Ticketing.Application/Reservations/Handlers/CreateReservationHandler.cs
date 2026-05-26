using MediatR;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Contracts.Reservations;
using Ticketing.Domain.Common.Exceptions;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Screenings.Exceptions;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Result<CreateReservationResponse>>
{
    private readonly IScreeningRepository _screenings;
    private readonly IReservationRepository _reservations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISeatLockService _seatLockService;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(
        IScreeningRepository screenings,
        IReservationRepository reservations,
        IUnitOfWork unitOfWork,
        ISeatLockService seatLockService,
        ILogger<CreateReservationHandler> logger)
    {
        _screenings = screenings;
        _reservations = reservations;
        _unitOfWork = unitOfWork;
        _seatLockService = seatLockService;
        _logger = logger;
    }

    public async Task<Result<CreateReservationResponse>> Handle(
        CreateReservationCommand request,
        CancellationToken cancellationToken)
    {
        if (request.SeatIds == null || request.SeatIds.Count == 0)
            return Result<CreateReservationResponse>.Failure("No seats selected");

        var screening = await _screenings.GetByIdAsync(request.ScreeningId);
        if (screening == null)
        {
            _logger.LogWarning("Screening {ScreeningId} not found", request.ScreeningId);
            return Result<CreateReservationResponse>.Failure("Screening not found");
        }

        var lockedSeats = new Dictionary<Guid, string>();
        foreach (var seatId in request.SeatIds)
        {
            var lockValue = await _seatLockService.TryLockSeatAsync(screening.Id, seatId);
            if (lockValue == null)
            {
                _logger.LogWarning("Seat {SeatId} already locked in screening {ScreeningId}",
                    seatId, screening.Id);

                foreach (var kv in lockedSeats)
                    await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);

                return Result<CreateReservationResponse>.Failure("Seat is already being reserved");
            }
            lockedSeats[seatId] = lockValue;
        }

        try
        {
            foreach (var seatId in lockedSeats.Keys)
                screening.GetSeat(seatId).Reserve();

            var reservation = new Reservation(Guid.NewGuid(), screening.EventId, screening.Id, [.. lockedSeats.Keys]);
            await _reservations.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reservation {ReservationId} created for screening {ScreeningId} with {SeatCount} seat(s)",
                reservation.Id, screening.Id, lockedSeats.Count);

            return Result<CreateReservationResponse>.Success(new CreateReservationResponse(reservation.Id));
        }
        catch (ScreeningSeatNotFoundException ex)
        {
            _logger.LogWarning(ex, "Seat not found in screening {ScreeningId}", screening.Id);
            return Result<CreateReservationResponse>.Failure("One or more seats do not exist in this screening.");
        }
        catch (ScreeningSeatNotAvailableException ex)
        {
            _logger.LogWarning(ex, "Seat not available in screening {ScreeningId}", screening.Id);
            return Result<CreateReservationResponse>.Failure("One or more seats are no longer available.");
        }
        catch (ConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict for screening {ScreeningId}", screening.Id);
            return Result<CreateReservationResponse>.Failure("Seats are no longer available.");
        }
        finally
        {
            foreach (var kv in lockedSeats)
                await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);
        }
    }
}