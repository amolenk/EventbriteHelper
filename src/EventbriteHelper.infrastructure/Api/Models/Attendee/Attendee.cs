using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Attendee
{
    public class Attendee
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("created")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("changed")]
        public DateTime ChangedOn { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ticket_class_name")]
        public string TicketClassName { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Created on: {CreatedOn:dd-MM-yyy HH:mm}, Changed on: {ChangedOn:dd-MM-yyy HH:mm}, Status: {Status}";
        }
    }
}
