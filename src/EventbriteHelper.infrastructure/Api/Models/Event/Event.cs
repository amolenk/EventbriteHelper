using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class Event
    {
        [JsonProperty("name")]
        public EventName EventName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("capacity")]
        public int Capacity { get; set; }

        [JsonProperty("ticket_classes")]
        public TicketClass[] TicketClasses { get; set; } = Array.Empty<TicketClass>();

        public override string ToString()
        {
            return $"EventName: {EventName.Name}, Capacity: {Capacity}, Amount sold: {TicketClasses[0].QuantitySold}";
        }
    }
}
