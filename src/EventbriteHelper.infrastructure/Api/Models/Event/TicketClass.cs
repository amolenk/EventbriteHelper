using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class TicketClass
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity_sold")]
        public int QuantitySold { get; set; }

        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
}
