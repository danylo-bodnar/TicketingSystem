using MediatR;
using Ticketing.Contracts.DTOs;

namespace Ticketing.Application.Screenings.Queries;

public record GetScreeningSeatsQuery(Guid ScreeningId)
    : IRequest<Result<List<SeatDto>>>;