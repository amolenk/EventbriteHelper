using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class TicketClassObject
    {
        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
}
