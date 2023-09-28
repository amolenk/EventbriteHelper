namespace EventbriteHelper.infrastructure.Configuration
{
    public class TableStorageConfiguration
    {
        public const string ConfigurationMappingName = "TableStorage";

        public string EventName { get; set; }
        public string EventYear { get; set; }
        public string AccountName { get; set; }
        public string StorageAccountKey { get; set; }
    }
}
