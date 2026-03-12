using System.ComponentModel.DataAnnotations;
using Ticketing.Domain.Screenings.Exceptions;

namespace Ticketing.Domain.Screenings
{
    public class ScreeningSeat
    {
        public Guid Id { get; private set; }

        public Guid ScreeningId { get; private set; }

        public Guid SeatId { get; private set; }

        public ScreeningSeatStatus Status { get; private set; }

        [Timestamp]
        public uint Version { get; private set; }

        private ScreeningSeat() { }

        public ScreeningSeat(Guid id, Guid screeningId, Guid seatId)
        {
            Id = id;
            ScreeningId = screeningId;
            SeatId = seatId;
            Status = ScreeningSeatStatus.Available;
        }

        public void Reserve()
        {
            if (Status != ScreeningSeatStatus.Available)
                throw new ScreeningSeatNotAvailableException(Id);

            Status = ScreeningSeatStatus.Reserved;
        }

        public void MarkAsSold()
        {
            if (Status != ScreeningSeatStatus.Reserved)
                throw new ScreeningSeatNotReservedException(Id);

            Status = ScreeningSeatStatus.Sold;
        }

        public void Release()
        {
            if (Status == ScreeningSeatStatus.Sold)
                throw new ScreeningSeatAlreadySoldException(Id);

            Status = ScreeningSeatStatus.Available;
        }
    }

    public enum ScreeningSeatStatus
    {
        Available = 0,
        Reserved = 1,
        Sold = 2
    }
}