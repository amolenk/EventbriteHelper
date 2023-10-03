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
    var optionsList = new Dictionary<int, string>
    {
        { 1, "Sync attendees with table storage" },
        { 2, "Normalize the ticket type capacity (to original capacity)" },
        { 3, "Set the ticket type capacity" },
        { 4, "Send registration confirmations" },
        { 5, "Set original capacity for ticket type" }
    };

    Console.Clear();

    var givenAnswer = 0;

    while (givenAnswer == 0)
    {
        Console.WriteLine("What do you want to do?");

        foreach (var options in optionsList)
        {
            Console.WriteLine($"{options.Key} - {options.Value}");
        }

        Console.WriteLine($"{optionsList.Count + 1} - Nothing. Close this application");

        var firstAnswer = Console.ReadLine();
        var ticketType = "";

        if (!string.IsNullOrEmpty(firstAnswer))
        {
            switch (firstAnswer)
            {
                case "1":
                    Console.WriteLine($"\n{optionsList[int.Parse(firstAnswer)]}\n");
                    givenAnswer = 1;
                    ticketType = await DetermineTicketTypeAsync(hostProvider, true);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

                    Console.WriteLine("Processing...\n");
                    await SyncAttendeesAsync(hostProvider, ticketType);
                    break;

                case "2":
                    Console.WriteLine($"\n{optionsList[int.Parse(firstAnswer)]}\n");
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
                    Console.WriteLine($"\n{optionsList[int.Parse(firstAnswer)]}\n");
                    givenAnswer = 3;

                    ticketType = await DetermineTicketTypeAsync(hostProvider);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

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

                        Console.WriteLine("Please enter a number.\n");
                    }

                    Console.WriteLine("Processing...\n");
                    await SetCapacity(hostProvider, ticketType, capacity);
                    break;

                case "4":
                    Console.WriteLine($"\n{optionsList[int.Parse(firstAnswer)]}\n");
                    givenAnswer = 4;
                    Console.WriteLine("Processing...\n");
                    SendRegistrationConfirmation();
                    break;

                case "5":
                    Console.WriteLine($"\n{optionsList[int.Parse(firstAnswer)]}\n");
                    givenAnswer = 5;

                    ticketType = await DetermineTicketTypeAsync(hostProvider);

                    if (string.IsNullOrEmpty(ticketType))
                    {
                        Console.WriteLine("There were no ticket types found.\n");
                        break;
                    }

                    var givenCorrectOriginalCapacity = false;

                    var originalCapacity = 0;

                    while (!givenCorrectOriginalCapacity)
                    {
                        Console.WriteLine("What do you want to be the original capacity?");

                        var response = Console.ReadLine();

                        if (int.TryParse(response, out capacity) && capacity > 0)
                        {
                            givenCorrectCapacity = true;
                            originalCapacity = capacity;
                            break;
                        }

                        Console.WriteLine("Please enter a number greater than 0.\n");
                    }

                    Console.WriteLine("Processing...\n");
                    SetOriginalCapacity(hostProvider, ticketType, originalCapacity);
                    break;

                case "6":
                    givenAnswer = 6;
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

async Task<string> DetermineTicketTypeAsync(IServiceProvider hostProvider, bool inclAll = false)
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

        var counter = 1;

        foreach (var type in ticketTypes)
        {
            Console.WriteLine($"{counter++} - {type}");
        }

        if (inclAll)
        {
            Console.WriteLine($"{counter} - For all ticket types");
        }

        var answer = Console.ReadLine();

        if (!string.IsNullOrEmpty(answer) && int.TryParse(answer, out var n))
        {
            if (n < ticketTypes.ToList().Count + 1)
            {
                ticketType = ticketTypes.ToList()[n - 1];
            }
            else
            {
                ticketType = "All";
            }

            Console.WriteLine($"\nChosen answer: {ticketType}\n");

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

void SetOriginalCapacity(IServiceProvider hostProvider, string ticketType, int originalCapacity)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    IAttendeeService attendeeService = provider.GetRequiredService<IAttendeeService>();

    attendeeService.SetOriginalCapacity(ticketType, originalCapacity);
}

async Task SyncAttendeesAsync(IServiceProvider hostProvider, string ticketType)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    IAttendeeService attendeeService = provider.GetRequiredService<IAttendeeService>();

    var attendeesResult = await attendeeService.GetAllAttendeesAsync();

    var ticketTypes = await attendeeService.SyncAttendeesAsync(attendeesResult.Attendees, ticketType);

    if (ticketType == "All")
    {
        foreach (var type in ticketTypes)
        {
            var attendees = attendeesResult.Attendees.Where(a => a.TicketClassName == type);
            var attendingAmount = attendees.Count(x => x.Status == "Attending");

            Console.WriteLine($"Processed {attendees.Count()} attendees for {type}. {attendingAmount} attending.");
        }
    }
    else
    {
        var attendingAmount = attendeesResult.Attendees.Count(x => x.Status == "Attending" && x.TicketClassName == ticketType);

        Console.WriteLine($"Processed {attendeesResult.Attendees.Count} attendees for {ticketType}. {attendingAmount} attending.");
    }
}

void SendRegistrationConfirmation()
{
    throw new NotImplementedException();
}
