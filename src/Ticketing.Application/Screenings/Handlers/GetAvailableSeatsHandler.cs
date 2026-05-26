using MediatR;
using Ticketing.Application.Common;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Contracts.DTOs;
using Ticketing.Domain.Halls;

public class GetAvailableSeatsHandler
    : IRequestHandler<GetAvailableSeatsQuery, Result<List<SeatDto>>>
{
    private readonly IScreeningRepository _screenings;
    private readonly IHallRepository _halls;

    public GetAvailableSeatsHandler(
        IScreeningRepository screeningRepository,
        IHallRepository hallRepository)
    {
        _screenings = screeningRepository;
        _halls = hallRepository;
    }

    public async Task<Result<List<SeatDto>>> Handle(
        GetAvailableSeatsQuery request,
        CancellationToken cancellationToken)
    {
        var seats = await _screenings.GetAvailableSeatsAsync(request.ScreeningId);

        if (!seats.Any())
            return Result<List<SeatDto>>.Failure("No available seats or screening not found");

        return Result<List<SeatDto>>.Success(seats.Select(s => new SeatDto
        {
            SeatId = s.SeatId,
            Row = s.Seat.Row,
            Column = s.Seat.Column,
            Status = s.Status,
        }).ToList());
    }
}
