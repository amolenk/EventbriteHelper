namespace EventbriteHelper.cli.tests.features.RegistrationConfirmation;

[TestClass]
public class RegistrationConfirmationTests
{
    [TestMethod]
    public void RegistrationConfirmation_should_run_on_right_param()
    {
        // arrange
        string[] args = new string[1]
        {
            "start-confirmation"
        };

        // act
        Program.Main(args);

        // assert
    }
}