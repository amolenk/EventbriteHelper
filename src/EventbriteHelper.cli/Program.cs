using EventbriteHelper.infrastructure;
using EventbriteHelper.infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices((hostbuilder, services) =>
    {
        services.AddInfrastructure(hostbuilder.Configuration);
    })
    .ConfigureAppConfiguration(app =>
    {
        app.AddJsonFile("local.settings.json");
    })
    .Build();

var keepRunning = true;

while (keepRunning)
{
    keepRunning = await StartProgram(host.Services);
}

await host.StopAsync();

async Task<bool> StartProgram(IServiceProvider hostProvider)
{
    Console.Clear();

    var givenAnswer = 0;

    while (givenAnswer == 0)
    {
        Console.WriteLine("What do you want to do?");
        Console.WriteLine("1 - Sync attendees with table storage");
        Console.WriteLine("2 - Normalize the ticket type capacity (to max tickets)");
        Console.WriteLine("3 - Set the ticket type capacity");
        Console.WriteLine("4 - Send registration confirmations");
        Console.WriteLine("5 - Nothing. Close this application");

        var firstAnswer = Console.ReadLine();
        var ticketType = "";

        if (!string.IsNullOrEmpty(firstAnswer))
        {
            switch (firstAnswer)
            {
                case "1":
                    givenAnswer = 1;
                    ticketType = await DetermineTicketTypeAsync(hostProvider);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

                    Console.WriteLine("Processing...\n");
                    await SyncAttendeesAsync(hostProvider, ticketType);
                    break;

                case "2":
                    ticketType = await DetermineTicketTypeAsync(hostProvider);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

                    givenAnswer = 2;

                    Console.WriteLine("Processing...\n");
                    await SetCapacity(hostProvider, ticketType);
                    break;

                case "3":
                    givenAnswer = 3;
                    var givenCorrectCapacity = false;

                    var capacity = 0;

                    while (!givenCorrectCapacity)
                    {
                        Console.WriteLine("What do you want to be the capacity?");

                        var response = Console.ReadLine();

                        if (int.TryParse(response, out capacity))
                        {
                            givenCorrectCapacity = true;
                            break;
                        }

                        Console.WriteLine("Please enter a number greater than 0.\n");
                    }

                    ticketType = await DetermineTicketTypeAsync(hostProvider);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

                    Console.WriteLine("Processing...\n");
                    await SetCapacity(hostProvider, ticketType, capacity);
                    break;

                case "4":
                    givenAnswer = 4;
                    Console.WriteLine("Processing...\n");
                    SendRegistrationConfirmation();
                    break;

                case "5":
                    givenAnswer = 5;
                    return false;

                default:
                    Console.WriteLine("Something went wrong. Please choose a number.\n");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Something went wrong. Please choose a number.\n");
        }
    }

    givenAnswer = 0;

    while (givenAnswer == 0)
    {
        Console.WriteLine("\nWould you like to take another action? (Y/N)");

        var secondAnswer = Console.ReadLine();

        if (!string.IsNullOrEmpty(secondAnswer))
        {
            switch (secondAnswer.ToLower())
            {
                case "y":
                    givenAnswer = 1;
                    break;

                case "n":
                    givenAnswer = 2;
                    break;

                default:
                    Console.WriteLine("Something went wrong. Please choose Y or N.\n");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Something went wrong. Please choose Y or N.\n");
        }
    }

    if (givenAnswer == 1)
    {
        return true;
    }

    return false;
}

async Task<string> DetermineTicketTypeAsync(IServiceProvider hostProvider)
{
    var givenAnswer = false;
    var ticketType = string.Empty;

    while (!givenAnswer)
    {
        Console.WriteLine("Retrieving ticket types...");

        using IServiceScope serviceScope = hostProvider.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        IAttendeeService attendeeService = provider.GetRequiredService<IAttendeeService>();

        var ticketTypes = await attendeeService.RetrieveTicketTypes();

        if (ticketTypes == null || !ticketTypes.Any())
        {
            return ticketType;
        }

        Console.WriteLine("\nFor which ticket type do you want to perform this action?");

        var counter = 0;

        foreach (var type in ticketTypes)
        {
            Console.WriteLine($"{counter + 1} - {type}");
        }

        Console.WriteLine();

        var answer = Console.ReadLine();

        if (!string.IsNullOrEmpty(answer) && int.TryParse(answer, out var n))
        {
            ticketType = ticketTypes.ToList()[n - 1];
            givenAnswer = true;
        }
        else
        {
            Console.WriteLine("Something went wrong. Please choose a number.\n");
        }
    }

    return ticketType;
}

async Task SetCapacity(IServiceProvider hostProvider, string ticketType, int capacity = 0)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    IAttendeeService attendeeService = provider.GetRequiredService<IAttendeeService>();

    await attendeeService.SetCapacityAsync(ticketType, capacity);
}

async Task SyncAttendeesAsync(IServiceProvider hostProvider, string ticketType)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    IAttendeeService attendeeService = provider.GetRequiredService<IAttendeeService>();

    var attendeesResult = await attendeeService.GetAllAttendeesAsync();

    await attendeeService.SyncAttendeesAsync(attendeesResult.Attendees, ticketType);

    var attendingAmount = attendeesResult.Attendees.Count(x => x.Status == "Attending");

    Console.WriteLine($"Checked {attendeesResult.Attendees.Count} attendees for {ticketType}. {attendingAmount} attending.");
}

void SendRegistrationConfirmation()
{
    throw new NotImplementedException();
}
