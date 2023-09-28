namespace EventbriteHelper.infrastructure.Azure.Models
{
    public class AttendeeInformation
    {
        public string EventId { get; set; }
        public string AttendeeId { get; set; }
        public DateTime InitialOrderDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool IsAttending { get; set; }
        public DateTime? ConfirmRequested { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public bool Confirmed { get; set; }
    }
}
