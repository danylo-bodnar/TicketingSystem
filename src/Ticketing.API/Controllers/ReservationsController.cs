using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Match(
                success => HandleCreatedResult(result, nameof(CreateReservation), new { id = success.ReservationId }),
                failure => HandleFailure(result)
            );
        }
    }
}