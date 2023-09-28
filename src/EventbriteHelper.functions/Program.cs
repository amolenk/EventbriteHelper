using EventbriteHelper.infrastructure;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices((hostbuilder, services) =>
    {
        services.AddInfrastructure(hostbuilder.Configuration);
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
