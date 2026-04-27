using MediatR;

namespace Ticketing.Application.Payments.Commands
{
    public record FailPaymentCommand(Guid PaymentId) : IRequest;
}