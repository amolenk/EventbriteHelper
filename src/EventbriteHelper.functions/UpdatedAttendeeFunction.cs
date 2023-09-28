using EventbriteHelper.infrastructure.Api.Models.Trigger;
using EventbriteHelper.infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace EventbriteHelper.functions
{
    public class UpdatedAttendeeFunction
    {
        private readonly ILogger _logger;
        private readonly IAttendeeService _attendeeService;

        public UpdatedAttendeeFunction(ILoggerFactory loggerFactory, IAttendeeService attendeeService)
        {
            _logger = loggerFactory.CreateLogger<UpdatedAttendeeFunction>();
            _attendeeService = attendeeService;
        }

        [Function("UpdatedAttendeeFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("Webhook trigger: An attendee was updated.");

            using var reader = new StreamReader(req.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();

            var triggerObject = JsonConvert.DeserializeObject<HttpTriggerObject>(body);
            var splitApiUrl = triggerObject.ApiUrl.Split("/");
            var attendeeId = splitApiUrl[^2];

            var ticketType = await _attendeeService.ProcessUpdatedAttendeeAsync(attendeeId);

            _logger.LogInformation($"Processed attendee update for {ticketType}.");

            return new OkResult();
        }
    }
}
