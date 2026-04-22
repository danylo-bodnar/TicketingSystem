using MediatR;
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

    public CreateReservationHandler(IScreeningRepository screenings, IReservationRepository reservations, IUnitOfWork unitOfWork, ISeatLockService seatLockService)
    {
        _screenings = screenings;
        _reservations = reservations;
        _unitOfWork = unitOfWork;
        _seatLockService = seatLockService;
    }

    public async Task<Result<CreateReservationResponse>> Handle(
     CreateReservationCommand request,
     CancellationToken cancellationToken)
    {
        if (request.SeatIds.Count == 0)
        {
            return Result<CreateReservationResponse>.Failure("No seats selected");
        }

        var screening = await _screenings.GetByIdAsync(request.ScreeningId);
        if (screening == null)
        {
            return Result<CreateReservationResponse>.Failure("Screening not found");
        }

        var lockedSeats = new Dictionary<Guid, string>();

        foreach (var seatId in request.SeatIds)
        {
            var lockValue = await _seatLockService.TryLockSeatAsync(screening.Id, seatId);

            if (lockValue == null)
            {
                foreach (var kv in lockedSeats)
                {
                    await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);
                }

                return Result<CreateReservationResponse>.Failure("Seat is already being reserved");
            }

            lockedSeats[seatId] = lockValue;
        }

        try
        {
            foreach (var seatId in lockedSeats.Keys)
            {
                var seat = screening.GetSeat(seatId);
                seat.Reserve();
            }

            var reservation = new Reservation(Guid.NewGuid(), screening.EventId, screening.Id, [.. lockedSeats.Keys]);

            await _reservations.AddAsync(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateReservationResponse>.Success(new CreateReservationResponse(reservation.Id));
        }
        catch (ScreeningSeatNotFoundException)
        {
            return Result<CreateReservationResponse>
                .Failure("One or more seats do not exist in this screening.");
        }
        catch (ScreeningSeatNotAvailableException)
        {
            return Result<CreateReservationResponse>.Failure("One or more seats are no longer available.");
        }
        catch (ConcurrencyException)
        {
            return Result<CreateReservationResponse>.Failure("Seats are no longer available.");
        }
        finally
        {
            foreach (var kv in lockedSeats)
                await _seatLockService.ReleaseSeatAsync(screening.Id, kv.Key, kv.Value);
        }
    }
}
