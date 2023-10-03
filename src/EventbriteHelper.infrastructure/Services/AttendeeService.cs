using EventbriteHelper.infrastructure.Api;
using EventbriteHelper.infrastructure.Api.Models.Attendee;
using EventbriteHelper.infrastructure.Api.Models.Event;
using EventbriteHelper.infrastructure.Azure;
using EventbriteHelper.infrastructure.Azure.Models;

namespace EventbriteHelper.infrastructure.Services
{
    public class AttendeeService : IAttendeeService
    {
        private readonly IEventbriteClient _eventbriteClient;
        private readonly ITableStorageClient _tableStorageClient;

        public AttendeeService(IEventbriteClient eventbriteClient, ITableStorageClient tableStorageClient)
        {
            _eventbriteClient = eventbriteClient ?? throw new ArgumentNullException(nameof(eventbriteClient));
            _tableStorageClient = tableStorageClient ?? throw new ArgumentNullException(nameof(tableStorageClient));
        }

        public async ValueTask<AllAttendees> GetAllAttendeesAsync()
        {
            var attendees = await _eventbriteClient.GetInformation<AllAttendees>("attendees/");

            return attendees;
        }

        public void UpsertAttendee(Attendee attendee)
        {
            var attendeeInformation = new AttendeeInformation()
            {
                EventId = attendee.EventId,
                AttendeeId = attendee.Id,
                IsAttending = attendee.Status == "Attending",
                Confirmed = false,
                ConfirmedDate = null,
                ConfirmRequested = null,
                InitialOrderDate = attendee.CreatedOn,
                LastUpdatedDate = attendee.ChangedOn
            };

            _tableStorageClient.AddOrUpdateAttendee(attendeeInformation, attendee.TicketClassName);
        }

        public async ValueTask<List<string>> SyncAttendeesAsync(List<Attendee> attendees, string ticketType)
        {
            foreach (var attendee in attendees)
            {
                UpsertAttendee(attendee);
            }

            var eventInformation = await _eventbriteClient.GetInformation<Event>("?expand=ticket_classes");

            if (ticketType == "All")
            {
                foreach (var tClass in eventInformation.TicketClasses)
                {
                    var ticketClass = eventInformation.TicketClasses.First(x => x.Name == tClass.Name);

                    var quantityDifference = ticketClass.Capacity - ticketClass.QuantitySold;
                    AdjustTicketTypeStatusBasedOnQuantityDifference(quantityDifference, ticketClass.Name);
                }
            }
            else
            {
                var ticketClass = eventInformation.TicketClasses.First(x => x.Name == ticketType);

                var quantityDifference = ticketClass.Capacity - ticketClass.QuantitySold;
                AdjustTicketTypeStatusBasedOnQuantityDifference(quantityDifference, ticketType);
            }

            return eventInformation.TicketClasses.Select(t => t.Name).ToList();
        }

        public async ValueTask<string> ProcessUpdatedAttendeeAsync(string attendeeId)
        {
            // Get attendee and update table storage
            var attendee = await _eventbriteClient.GetInformation<Attendee>($"attendees/{attendeeId}/");
            UpsertAttendee(attendee);

            // Get event information and update table storage and event if event is sold out
            await UpdateCapacityAndStatusIfNeededAsync(attendee.Status, attendee.TicketClassName);

            return attendee.TicketClassName;
        }

        public async Task SetCapacityAsync(string ticketType, int capacity = 0)
        {
            var eventInformation = await _eventbriteClient.GetInformation<Event>("?expand=ticket_classes");

            var ticketClass = eventInformation.TicketClasses.First(x => x.Name == ticketType);

            Console.WriteLine($"Before - Capacity: {ticketClass.Capacity}, Tickets sold: {ticketClass.QuantitySold}/{ticketClass.Capacity}");

            if (capacity == 0)
            {
                var originalCapacity = _tableStorageClient.GetOriginalCapacity(ticketType);
                ticketClass = await AdjustTicketTypeCapacityAsync(ticketClass.Id, originalCapacity);
            }
            else
            {
                ticketClass = await AdjustTicketTypeCapacityAsync(ticketClass.Id, capacity);
            }

            var quantityDifference = ticketClass.Capacity - ticketClass.QuantitySold;
            AdjustTicketTypeStatusBasedOnQuantityDifference(quantityDifference, ticketType);

            Console.WriteLine($"Capacity set for {ticketType} - Capacity: {ticketClass.Capacity}, Tickets sold: {ticketClass.QuantitySold}/{ticketClass.Capacity}");
        }

        public void SetOriginalCapacity(string ticketType, int originalCapacity)
        {
            _tableStorageClient.SetOriginalCapacity(ticketType, originalCapacity);

            Console.WriteLine($"Original capacity set for {ticketType}: {originalCapacity}");
        }

        public async Task<IEnumerable<string>> RetrieveTicketTypes()
        {
            var eventInformation = await _eventbriteClient.GetInformation<Event>("?expand=ticket_classes");

            var ticketTypes = new List<string>();

            foreach (var ticketClass in eventInformation.TicketClasses)
            {
                ticketTypes.Add(ticketClass.Name);
            }

            return ticketTypes;
        }

        private async ValueTask<TicketClass> AdjustTicketTypeCapacityAsync(string ticketTypeId, int capacity)
        {
            return await _eventbriteClient.AdjustTicketTypeCapacity(ticketTypeId, capacity);
        }

        private void AdjustTicketTypeStatusBasedOnQuantityDifference(int quantityDifference, string ticketType)
        {
            if (quantityDifference <= 0)
            {
                _tableStorageClient.SetTicketTypeStatus(Status.SoldOut, ticketType);
            }
            else
            {
                _tableStorageClient.SetTicketTypeStatus(Status.Open, ticketType);
            }
        }

        private async ValueTask UpdateCapacityAndStatusIfNeededAsync(string attendeeStatus, string ticketType)
        {
            var eventInformation = await _eventbriteClient.GetInformation<Event>("?expand=ticket_classes");

            var ticketClass = eventInformation.TicketClasses.First(x => x.Name == ticketType);

            if (attendeeStatus == "Attending")
            {
                if (ticketClass.QuantitySold == ticketClass.Capacity)
                {
                    _tableStorageClient.SetTicketTypeStatus(Status.SoldOut, ticketType);

                    if (ticketClass.Capacity < ticketClass.Capacity)
                    {
                        await _eventbriteClient.AdjustTicketTypeCapacity(ticketClass.Id, ticketClass.Capacity + 1);
                    }

                }
                else
                {
                    if (ticketClass.Capacity < ticketClass.QuantitySold)
                    {
                        await _eventbriteClient.AdjustTicketTypeCapacity(ticketClass.Id, ticketClass.QuantitySold);
                    }
                }

            }
            else
            {
                var currentStatus = _tableStorageClient.GetTicketTypeStatus(ticketType).Status;

                if (currentStatus != Status.Open)
                {
                    await _eventbriteClient.AdjustTicketTypeCapacity(ticketClass.Id, ticketClass.QuantitySold);

                    if (currentStatus == Status.SoldOut)
                    {
                        _tableStorageClient.SetTicketTypeStatus(Status.Adjusted, ticketType);
                    }
                }
            }
        }
    }
}
