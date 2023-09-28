using EventbriteHelper.infrastructure.Azure.Models;

namespace EventbriteHelper.infrastructure.Azure
{
    public interface ITableStorageClient
    {
        void AddOrUpdateAttendee(AttendeeInformation attendee, string ticketType);
        IEnumerable<AttendeeInformation> GetAttendingAttendees(string eventId, string ticketType);
        TicketTypeStatus GetTicketTypeStatus(string ticketType);
        void SetTicketTypeStatus(Status status, string ticketType);
    }
}
