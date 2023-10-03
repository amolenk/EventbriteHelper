using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class TicketClassUpdate
    {
        [JsonProperty("ticket_class")]
        public TicketClassObject TicketClassCapacity { get; set; }

        public TicketClassUpdate(int capacity)
        {
            TicketClassCapacity = new TicketClassObject { Capacity = capacity };
        }
    }
}
