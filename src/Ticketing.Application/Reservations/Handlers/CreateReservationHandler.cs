using MediatR;
using Ticketing.Application.Common;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Contracts.Reservations;
using Ticketing.Domain.Common.Exceptions;
using Ticketing.Domain.Reservations;

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
        var screening = await _screenings.GetByIdAsync(request.ScreeningId);
        if (screening == null)
            return Result<CreateReservationResponse>.Failure("Screening not found");

        var reservedSeats = new List<Guid>();
        foreach (var seatId in request.SeatIds)
        {
            var lockAcquired = await _seatLockService.TryLockSeatAsync(screening.Id, seatId);

            if (!lockAcquired)
                return Result<CreateReservationResponse>.Failure($"Seat {seatId} is already being reserved");

            var seat = screening.GetSeat(seatId);
            seat.Reserve();
            reservedSeats.Add(seat.SeatId);
        }

        var reservation = new Reservation(Guid.NewGuid(), screening.EventId, screening.Id, reservedSeats);
        await _reservations.AddAsync(reservation);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            return Result<CreateReservationResponse>.Failure("Seats are no longer available.");
        }
        finally
        {
            foreach (var seatId in reservedSeats)
            {
                await _seatLockService.ReleaseSeatAsync(screening.Id, seatId);
            }
        }

        return Result<CreateReservationResponse>.Success(new CreateReservationResponse(reservation.Id));
    }
}