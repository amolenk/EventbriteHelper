namespace EventbriteHelper.infrastructure.Azure.Models
{
    public class TicketTypeInformation
    {
        public Status Status { get; set; }
        public int OriginalCapacity { get; set; }
    }

    public enum Status
    {
        Open,
        SoldOut,
        Adjusted
    }
}
