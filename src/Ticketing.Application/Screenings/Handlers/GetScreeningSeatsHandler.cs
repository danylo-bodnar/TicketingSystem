using MediatR;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Contracts.DTOs;

namespace Ticketing.Application.Screenings.Handlers;

public class GetScreeningSeatsHandler
    : IRequestHandler<GetScreeningSeatsQuery, Result<List<SeatDto>>>
{
    private readonly IScreeningRepository _screenings;

    public GetScreeningSeatsHandler(IScreeningRepository screenings)
    {
        _screenings = screenings;
    }

    public async Task<Result<List<SeatDto>>> Handle(
        GetScreeningSeatsQuery request,
        CancellationToken cancellationToken)
    {
        var screening = await _screenings.GetSeats(
            request.ScreeningId,
            cancellationToken);

        if (screening == null)
            return Result<List<SeatDto>>.Failure("Screening not found");

        var seats = screening.Seats.Select(s => new SeatDto
        {
            SeatId = s.SeatId,
            Row = s.Seat.Row,
            Column = s.Seat.Column,
            Status = s.Status
        }).ToList();

        return Result<List<SeatDto>>.Success(seats);
    }
}