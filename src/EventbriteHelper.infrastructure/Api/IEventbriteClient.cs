using EventbriteHelper.infrastructure.Api.Models.Event;

namespace EventbriteHelper.infrastructure.Api
{
    public interface IEventbriteClient
    {
        Task<T> GetInformation<T>(string requestUrl);
        Task<Event> AdjustTicketTypeCapacity(string ticketType, int newCapacity);
    }
}
