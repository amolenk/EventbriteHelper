using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class CapacityUpdate
    {
        [JsonProperty("event")]
        public EventCapacity EventCapacity { get; set; }

        public CapacityUpdate(int capacity)
        {
            EventCapacity = new EventCapacity { Capacity = capacity };
        }
    }
}
