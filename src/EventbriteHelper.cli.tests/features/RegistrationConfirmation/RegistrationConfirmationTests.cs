using NSubstitute;

namespace EventbriteHelper.cli.tests.features.RegistrationConfirmation;

[TestClass]
public class RegistrationConfirmationTests
{
    [TestMethod]
    public async Task RegistrationConfirmation_should_send_email_to_expectedAttendees()
    {
        // arrange
        var someEventbriteAPIMock = Substitute.For<IEventbriteAPI>();

        var attendeeService = new AttendeeService(someEventbriteAPIMock);

        var sut = new RegistrationConfirmationHandler(attendeeService);

        // act
        await sut.SendRegistrationConfirmation();

        // assert
        
    }

    // GET ALL ATTENDEES
    // STORE IN DB
    // SEND EMAIL WITH UNIQUE LINK
}