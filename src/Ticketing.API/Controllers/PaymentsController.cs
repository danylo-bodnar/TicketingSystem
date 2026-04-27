using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Payments.Commands;
using Ticketing.Contracts.DTOs;

namespace Ticketing.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymentWebhookDto dto)
        {
            if (dto.Status == "completed")
            {
                await _mediator.Send(new CompletePaymentCommand(dto.PaymentId));
            }
            else if (dto.Status == "failed")
            {
                await _mediator.Send(new FailPaymentCommand(dto.PaymentId));
            }
            else
            {
                return BadRequest("Unknown status");
            }

            return Ok();
        }
    }
}