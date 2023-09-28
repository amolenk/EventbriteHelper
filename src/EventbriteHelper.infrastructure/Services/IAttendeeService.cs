using EventbriteHelper.infrastructure.Api.Models.Attendee;

namespace EventbriteHelper.infrastructure.Services
{
    public interface IAttendeeService
    {
        ValueTask<AllAttendees> GetAllAttendeesAsync();
        void UpsertAttendee(Attendee attendee);
        ValueTask SyncAttendeesAsync(List<Attendee> attendees, string ticketType);
        ValueTask<string> ProcessUpdatedAttendeeAsync(string attendeeId);
        Task SetCapacityAsync(string ticketType, int capacity = 0);
        Task<IEnumerable<string>> RetrieveTicketTypes();
    }
}