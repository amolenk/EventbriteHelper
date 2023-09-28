namespace EventbriteHelper.infrastructure.Azure.Models
{
    public class TicketTypeStatus
    {
        public Status Status { get; set; }
    }

    public enum Status
    {
        Open,
        Vol,
        Aangepast
    }
}
