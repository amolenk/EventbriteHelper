using EventbriteHelper.infrastructure.Api;
using EventbriteHelper.infrastructure.Azure;
using EventbriteHelper.infrastructure.Services;
using NSubstitute;

namespace EventbriteHelper.cli.tests.features.RegistrationConfirmation;

[TestClass]
public class RegistrationConfirmationTests
{
    [TestMethod]
    public async Task RegistrationConfirmation_should_send_email_to_expectedAttendees()
    {
        // arrange
        var someEventbriteAPIMock = Substitute.For<EventbriteClient>();
        var someTableStorageAPIMock = Substitute.For<TableStorageClient>();

        var attendeeService = new AttendeeService(someEventbriteAPIMock, someTableStorageAPIMock);

        //var sut = new RegistrationConfirmationHandler(attendeeService);

        // act
        //await sut.SendRegistrationConfirmation();

        // assert

    }

    // GET ALL ATTENDEES
    // STORE IN DB
    // SEND EMAIL WITH UNIQUE LINK
}