using Newtonsoft.Json;

namespace EventbriteHelper.infrastructure.Api.Models.Attendee
{
    public class AllAttendees
    {
        [JsonProperty("attendees")]
        public List<Attendee> Attendees { get; set; }
    }
}
