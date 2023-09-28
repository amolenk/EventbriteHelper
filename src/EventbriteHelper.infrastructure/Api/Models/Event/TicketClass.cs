using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class TicketClass
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity_total")]
        public int QuantityTotal { get; set; }

        [JsonProperty("quantity_sold")]
        public int QuantitySold { get; set; }
    }
}
