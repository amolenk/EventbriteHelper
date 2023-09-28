using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Event
{
    public class EventName
    {
        [JsonProperty("text")]
        public string Name { get; set; }
    }
}
