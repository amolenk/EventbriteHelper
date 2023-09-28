using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Trigger
{
    public class HttpTriggerObject
    {
        [JsonProperty("config")]
        public TriggerData Data { get; set; }

        [JsonProperty("api_url")]
        public string ApiUrl { get; set; }

        public override string ToString()
        {
            return $"Action: {Data.Action}, UserId: {Data.UserId}, ApiUrl: {ApiUrl}";
        }
    }
}
