using Ticketing.Domain.Events;

namespace Ticketing.UnitTests.Common;

public class AggregateRootTests
{
    private class TestAggregate : AggregateRoot
    {
        public void RaiseEvent() => AddDomainEvent(new ReservationCreated
        {
            ReservationId = Guid.NewGuid(),
            ScreeningId = Guid.NewGuid(),
            SeatIds = [Guid.NewGuid()],
            CreatedAt = DateTime.UtcNow
        });

        public void RaiseMultiple(int count)
        {
            for (int i = 0; i < count; i++)
                RaiseEvent();
        }
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEvent()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseEvent();

        Assert.Single(aggregate.DomainEvents);
        Assert.IsType<ReservationCreated>(aggregate.DomainEvents.First());
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseEvent();

        var events = aggregate.DomainEvents;
        Assert.Single(events);

        aggregate.ClearDomainEvents();
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void AddDomainEvent_ShouldAccumulateMultipleEvents()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseMultiple(3);

        Assert.Equal(3, aggregate.DomainEvents.Count);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAll()
    {
        var aggregate = new TestAggregate();
        aggregate.RaiseMultiple(3);

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }
}
