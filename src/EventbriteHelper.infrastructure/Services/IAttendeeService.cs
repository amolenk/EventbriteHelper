using EventbriteHelper.infrastructure.Api.Models.Attendee;

namespace EventbriteHelper.infrastructure.Services
{
    public interface IAttendeeService
    {
        ValueTask<AllAttendees> GetAllAttendeesAsync();
        void UpsertAttendee(Attendee attendee);
        ValueTask<List<string>> SyncAttendeesAsync(List<Attendee> attendees, string ticketType);
        ValueTask<string> ProcessUpdatedAttendeeAsync(string attendeeId);
        Task SetCapacityAsync(string ticketType, int capacity = 0);
        void SetOriginalCapacity(string ticketType, int originalCapacity);
        Task<IEnumerable<string>> RetrieveTicketTypes();
    }
}