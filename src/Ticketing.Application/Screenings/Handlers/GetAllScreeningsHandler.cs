using MediatR;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Screenings.Queries;
using Ticketing.Contracts.Screenings;

namespace Ticketing.Application.Screenings.Handlers;

public class GetAllScreeningsHandler : IRequestHandler<GetAllScreeningsQuery, Result<List<ScreeningResponse>>>
{
    private readonly IScreeningRepository _screeningRepository;

    public GetAllScreeningsHandler(IScreeningRepository screeningRepository)
    {
        _screeningRepository = screeningRepository;
    }

    public async Task<Result<List<ScreeningResponse>>> Handle(GetAllScreeningsQuery request, CancellationToken cancellationToken)
    {
        var screenings = await _screeningRepository.GetAllAsync();

        var response = screenings.Select(s => new ScreeningResponse(
          s.Id,
          s.EventId,
          s.StartTime
        )).ToList();

        return Result<List<ScreeningResponse>>.Success(response);
    }

}
