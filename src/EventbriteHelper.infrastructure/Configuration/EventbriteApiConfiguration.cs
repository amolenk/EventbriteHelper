namespace EventbriteHelper.infrastructure.Configuration
{
    public class EventbriteApiConfiguration
    {
        public const string ConfigurationMappingName = "EventbriteApi";

        public string BaseAddress { get; set; }
        public string ApiKey { get; set; }
        public string EventId { get; set; }
    }
}
