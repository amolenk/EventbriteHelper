using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Trigger
{
    public class TriggerData
    {

        [JsonProperty("user_id")]
        public string UserId { get; set; }


        [JsonProperty("action")]
        public string Action { get; set; }
    }
}