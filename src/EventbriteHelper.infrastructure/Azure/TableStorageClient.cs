using Azure.Data.Tables;
using EventbriteHelper.infrastructure.Azure.Models;
using EventbriteHelper.infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventbriteHelper.infrastructure.Azure
{
    public class TableStorageClient : ITableStorageClient
    {
        private readonly ILogger _logger;
        private readonly string _eventName;
        private readonly string _eventYear;
        private readonly string _accountName;
        private readonly string _storageAccountKey;

        public TableStorageClient(IOptionsMonitor<TableStorageConfiguration> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TableStorageClient>();
            _eventName = options?.CurrentValue.EventName ?? throw new ArgumentNullException(nameof(options));
            _eventYear = options?.CurrentValue.EventYear ?? throw new ArgumentNullException(nameof(options));
            _accountName = options?.CurrentValue.AccountName ?? throw new ArgumentNullException(nameof(options));
            _storageAccountKey = options?.CurrentValue.StorageAccountKey ?? throw new ArgumentNullException(nameof(options));
        }

        public void AddOrUpdateAttendee(AttendeeInformation attendee, string ticketType)
        {
            var tableClient = GetClient(ticketType, "attendees");

            var tableEntity = new TableEntity(attendee.EventId, attendee.AttendeeId)
                {
                    { nameof(attendee.IsAttending), attendee.IsAttending },
                    { nameof(attendee.Confirmed), attendee.Confirmed },
                    { nameof(attendee.ConfirmedDate), attendee.ConfirmedDate },
                    { nameof(attendee.ConfirmRequested), attendee.ConfirmRequested },
                    { nameof(attendee.InitialOrderDate), attendee.InitialOrderDate },
                    { nameof(attendee.LastUpdatedDate), attendee.LastUpdatedDate },
                };

            tableClient.UpsertEntity(tableEntity);
            _logger.LogInformation($"Added/Updated attendee {attendee.AttendeeId} for {ticketType}");
        }

        // You can use this method to retrieve all attending attendees for a certain ticket type.
        // Then you can use the AttendeeId to retrieve the mailaddress and name from Eventbrite. (https://www.eventbrite.com/platform/docs/attendees)
        public IEnumerable<AttendeeInformation> GetAttendingAttendees(string eventId, string ticketType)
        {
            var tableClient = GetClient(ticketType, "attendees");

            var result = tableClient
                .Query<TableEntity>(entity =>
                    entity.PartitionKey == eventId &&
                    entity.GetBoolean("IsAttending") == true);

            var attendingAttendees = new List<AttendeeInformation>();

            foreach (var entity in result)
            {
                var attendeeInformation = new AttendeeInformation()
                {
                    EventId = entity.PartitionKey,
                    AttendeeId = entity.RowKey,
                    IsAttending = entity.GetBoolean("IsAttending") ?? throw new NullReferenceException("IsAttending is unknown"),
                    Confirmed = entity.GetBoolean("Confirmed") ?? throw new NullReferenceException("Confirmed is unknown"),
                    ConfirmedDate = entity.GetDateTime("Confirmed") ?? DateTime.MinValue,
                    ConfirmRequested = entity.GetDateTime("Confirmed") ?? DateTime.MinValue,
                    InitialOrderDate = entity.GetDateTime("InitialOrderDate") ?? DateTime.MinValue,
                    LastUpdatedDate = entity.GetDateTime("LastUpdatedDate") ?? DateTime.MinValue
                };

                attendingAttendees.Add(attendeeInformation);
            }

            return attendingAttendees;
        }

        public TicketTypeInformation GetTicketTypeStatus(string ticketType)
        {
            var tableClient = GetClient(ticketType, "information");
            var result = tableClient
                .Query<TableEntity>(entity =>
                    entity.PartitionKey == _eventName &&
                    entity.RowKey == _eventYear);

            if (result == null || !result.Any())
            {
                throw new NullReferenceException($"The event status for {_eventName} ({_eventYear}) does not yet exist and can't be retrieved.");
            }

            var currentStatus = Status.Open;

            if (!Enum.TryParse(result.First().GetString("Status"), out currentStatus))
            {
                throw new FormatException($"Something went wrong with trying to change the status for {_eventName} ({_eventYear})");
            }

            var ticketTypeInformation = new TicketTypeInformation
            {
                Status = currentStatus
            };

            return ticketTypeInformation;
        }

        public void SetTicketTypeStatus(Status status, string ticketType)
        {
            var ticketTypeInformation = GetTicketTypeStatus(ticketType);
            ticketTypeInformation.Status = status;

            var tableEntity = new TableEntity(_eventName, _eventYear)
            {
                    { nameof(ticketTypeInformation.Status), ticketTypeInformation.Status.ToString() },
            };

            var tableClient = GetClient(ticketType, "information");
            tableClient.UpsertEntity(tableEntity);
            _logger.LogInformation($"Updated status in {tableClient.Name}: {ticketTypeInformation.Status}");
        }

        public void SetOriginalCapacity(string ticketType, int originalCapacity)
        {
            var tableClient = GetClient(ticketType, "information");
            var result = tableClient
                .Query<TableEntity>(entity =>
                    entity.PartitionKey == _eventName &&
                    entity.RowKey == _eventYear);

            var tableEntity = new TableEntity(_eventName, _eventYear)
                {
                    { "OriginalCapacity", originalCapacity.ToString() },
                };

            tableClient.UpsertEntity(tableEntity);
        }

        public int GetOriginalCapacity(string ticketType)
        {
            var tableClient = GetClient(ticketType, "information");
            var result = tableClient
                .Query<TableEntity>(entity =>
                    entity.PartitionKey == _eventName &&
                    entity.RowKey == _eventYear);

            var originalCapacity = result.First().GetString("OriginalCapacity");

            if (int.TryParse(originalCapacity, out int capacity))
            {
                return capacity;
            }
            else
            {
                throw new FormatException($"Something went wrong with getting the original capacity from {tableClient.Name}");
            }
        }

        private TableClient GetClient(string ticketType, string tableType)
        {
            var tableName = CreateTableName(ticketType, tableType);

            // Attendees table
            var tableClient = new TableClient(
                new Uri($"https://{_accountName}.table.core.windows.net"),
                tableName,
                new TableSharedKeyCredential(_accountName, _storageAccountKey));

            tableClient.CreateIfNotExists();

            if (tableType == "information")
            {
                var result = tableClient
                .Query<TableEntity>(entity =>
                    entity.PartitionKey == _eventName &&
                    entity.RowKey == _eventYear);

                if (result == null || !result.Any())
                {
                    var information = new TicketTypeInformation { Status = Status.Open };

                    var tableEntity = new TableEntity(_eventName, _eventYear)
                    {
                        { nameof(information.Status), information.Status.ToString() },
                        { nameof(information.OriginalCapacity), string.Empty },
                    };

                    tableClient.UpsertEntity(tableEntity);
                    _logger.LogInformation($"Created row in {tableName} - Status: {information.Status}, Original Capacity: {information.OriginalCapacity}");
                }
            }

            return tableClient;
        }

        private string CreateTableName(string ticketType, string tableSort)
        {
            var trimmedTicketType = ticketType.Replace(" ", "");
            var tableName = $"{_eventName.ToLower()}{_eventYear}{trimmedTicketType.ToLower()}{tableSort}";

            if (tableName.Length > 60)
            {
                var neededTicketTypeLength = 60 - $"{_eventName.ToLower()}{_eventYear}{tableSort}".Length;
                var shortenedTicketTypeName = trimmedTicketType[..neededTicketTypeLength].ToLower();

                tableName = $"{_eventName.ToLower()}{_eventYear}{shortenedTicketTypeName}{tableSort}";
            }

            return tableName;
        }
    }
}
