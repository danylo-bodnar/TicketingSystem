
using MediatR;
using Ticketing.Application.Common;
using Ticketing.Contracts.Screenings;

namespace Ticketing.Application.Screenings.Queries;

public record GetAllScreeningsQuery : IRequest<Result<List<ScreeningResponse>>>
{

}
