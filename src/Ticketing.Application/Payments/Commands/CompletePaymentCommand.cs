using MediatR;

namespace Ticketing.Application.Payments.Commands
{
    public record CompletePaymentCommand(Guid PaymentId) : IRequest;
}