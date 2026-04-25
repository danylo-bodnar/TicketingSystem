using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Events;
using Ticketing.Domain.Common.Exceptions;
using Ticketing.Domain.Common.ValueObjects;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Screenings.Exceptions;

public class ReservationCreatedHandler : IEventHandler<ReservationCreated>
{
    private readonly IPaymentRepository _payments;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IScreeningRepository _screenings;
    private readonly PricingService _pricing = new();

    public ReservationCreatedHandler(IPaymentRepository payments, IUnitOfWork unitOfWork, IScreeningRepository screenings)
    {
        _payments = payments;
        _unitOfWork = unitOfWork;
        _screenings = screenings;
    }

    public async Task HandleAsync(ReservationCreated @event, CancellationToken ct)
    {
        var screening = await _screenings.GetByIdAsync(@event.ScreeningId, ct);
        if (screening == null)
            throw new ScreeningNotFoundException(@event.ScreeningId);

        var amount = _pricing.Calculate(
            seatCount: @event.SeatIds.Count,
            occupancyRatio: screening.GetOccupancyRatio());

        var payment = new Payment(@event.ReservationId, amount);
        await _payments.AddAsync(payment);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DuplicateEntityException) { }
    }
}

