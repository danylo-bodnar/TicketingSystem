using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments;

public class ReservationCreatedHandler : IEventHandler<ReservationCreated>
{
    private readonly IPaymentRepository _payments;

    private readonly IUnitOfWork _unitOfWork;

    public ReservationCreatedHandler(IPaymentRepository payment, IUnitOfWork unitOfWork)
    {
        _payments = payment;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ReservationCreated @event)
    {
        var amount = new Money(@event.SeatIds.Count * 100, "USD");

        var payment = new Payment(@event.ReservationId, amount);

        await _payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}
