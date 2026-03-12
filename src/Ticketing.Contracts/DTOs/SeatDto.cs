using Ticketing.Domain.Screenings;

namespace Ticketing.Contracts.DTOs
{
    public class SeatDto
    {
        public Guid SeatId { get; set; }
        public string Row { get; set; } = default!;
        public int Column { get; set; }
        public ScreeningSeatStatus Status { get; set; }
    }
}