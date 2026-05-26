using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Screenings.Queries;

namespace Ticketing.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreeningsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ScreeningsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}/available-seats")]
        public async Task<IActionResult> GetAvailableSeats(Guid id)
        {
            var result = await _mediator.Send(new GetAvailableSeatsQuery(id));
            return HandleResult(result);
        }

        [HttpGet("{id}/seats")]
        public async Task<IActionResult> GetSeats(Guid id)
        {
            var result = await _mediator.Send(new GetScreeningSeatsQuery(id));

            return HandleResult(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllScreeningsQuery());
            return HandleResult(result);
        }
    }
}
