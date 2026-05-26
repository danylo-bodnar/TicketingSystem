using MediatR;
using Ticketing.Contracts.DTOs;

namespace Ticketing.Application.Screenings.Queries;

public record GetAvailableSeatsQuery(Guid ScreeningId) : IRequest<Result<List<SeatDto>>>;
