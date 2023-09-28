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

        public async ValueTask SyncAttendeesAsync(List<Attendee> attendees, string ticketType)
        {
            foreach (var attendee in attendees)
            {
                UpsertAttendee(attendee);
            }

            var eventInformation = await _eventbriteClient.GetInformation<Event>("?expand=ticket_classes");

            var ticketClass = eventInformation.TicketClasses.First(x => x.Name == ticketType);

            var quantityDifference = ticketClass.QuantityTotal - ticketClass.QuantitySold;
            AdjustTicketTypeStatusBasedOnQuantityDifference(quantityDifference, ticketType);
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

            var updatedEvent = new Event();

            if (capacity == 0)
            {
                updatedEvent = await AdjustTicketTypeCapacityAsync(ticketType, ticketClass.QuantityTotal);
            }
            else
            {
                updatedEvent = await AdjustTicketTypeCapacityAsync(ticketType, capacity);
            }

            var quantityDifference = ticketClass.QuantityTotal - ticketClass.QuantitySold;
            AdjustTicketTypeStatusBasedOnQuantityDifference(quantityDifference, ticketType);

            Console.WriteLine($"Before - Capacity: {eventInformation.Capacity}, Tickets sold: {ticketClass.QuantitySold}/{ticketClass.QuantityTotal}");
            Console.WriteLine($"Capacity normalized - Capacity: {updatedEvent.Capacity}, Tickets sold: {ticketClass.QuantitySold}/{ticketClass.QuantityTotal}");
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

        private async ValueTask<Event> AdjustTicketTypeCapacityAsync(string ticketType, int capacity)
        {
            return await _eventbriteClient.AdjustTicketTypeCapacity(ticketType, capacity);
        }

        private void AdjustTicketTypeStatusBasedOnQuantityDifference(int quantityDifference, string ticketType)
        {
            if (quantityDifference == 0)
            {
                _tableStorageClient.SetTicketTypeStatus(Status.Vol, ticketType);
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
                if (ticketClass.QuantitySold == ticketClass.QuantityTotal)
                {
                    _tableStorageClient.SetTicketTypeStatus(Status.Vol, ticketType);

                    if (eventInformation.Capacity < ticketClass.QuantityTotal)
                    {
                        await _eventbriteClient.AdjustTicketTypeCapacity(ticketType, eventInformation.Capacity + 1);
                    }

                }
                else
                {
                    if (eventInformation.Capacity < ticketClass.QuantitySold)
                    {
                        await _eventbriteClient.AdjustTicketTypeCapacity(ticketType, ticketClass.QuantitySold);
                    }
                }

            }
            else
            {
                var currentStatus = _tableStorageClient.GetTicketTypeStatus(ticketType).Status;

                if (currentStatus != Status.Open)
                {
                    await _eventbriteClient.AdjustTicketTypeCapacity(ticketType, ticketClass.QuantitySold);

                    if (currentStatus == Status.Vol)
                    {
                        _tableStorageClient.SetTicketTypeStatus(Status.Aangepast, ticketType);
                    }
                }
            }
        }
    }
}
