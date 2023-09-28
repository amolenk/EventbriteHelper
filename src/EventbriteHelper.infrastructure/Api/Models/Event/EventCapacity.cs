using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class EventCapacity
    {
        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
}