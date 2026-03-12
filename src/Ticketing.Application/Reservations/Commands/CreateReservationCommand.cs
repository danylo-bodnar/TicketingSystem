using MediatR;
using Ticketing.Application.Common;
using Ticketing.Contracts.Reservations;

public record CreateReservationCommand(Guid ScreeningId, List<Guid> SeatIds)
    : IRequest<Result<CreateReservationResponse>>;