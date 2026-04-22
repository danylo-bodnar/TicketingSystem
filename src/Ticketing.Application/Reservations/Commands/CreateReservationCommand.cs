using MediatR;
using Ticketing.Contracts.Reservations;

public record CreateReservationCommand(Guid ScreeningId, List<Guid> SeatIds)
    : IRequest<Result<CreateReservationResponse>>;