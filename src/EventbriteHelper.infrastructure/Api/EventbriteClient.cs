using EventbriteHelper.infrastructure.Api.Models.Event;
using EventbriteHelper.infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EventbriteHelper.infrastructure.Api;

public class EventbriteClient : IEventbriteClient
{
    private readonly string _baseAddress;
    private readonly string _apiKey;
    private readonly string _eventId;

    public EventbriteClient(IOptionsMonitor<EventbriteApiConfiguration> options)
    {
        _baseAddress = options?.CurrentValue.BaseAddress ?? throw new ArgumentNullException(nameof(options));
        _apiKey = options?.CurrentValue.ApiKey ?? throw new ArgumentNullException(nameof(options));
        _eventId = options?.CurrentValue.EventId ?? throw new ArgumentNullException(nameof(options));
    }

    public EventbriteClient()
    {
        _baseAddress = "https://www.eventbriteapi.com/v3/";
        _apiKey = "OIDZZGLWF4X7STGXF526";
        _eventId = "710249795257";
    }

    public async Task<T> GetInformation<T>(string requestUrl)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_baseAddress)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var request = new HttpRequestMessage(HttpMethod.Get, $"events/{_eventId}/{requestUrl}");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
        catch (Exception ex)
        {
            throw new JsonException(ex.Message);
        }
    }

    // TO DO: Adjust the correct capacity for ticket type x
    public async Task<Event> AdjustTicketTypeCapacity(string ticketType, int newCapacity)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(_baseAddress)
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var request = new HttpRequestMessage(HttpMethod.Post, $"events/{_eventId}/");

        var capacityUpdate = new CapacityUpdate(newCapacity);
        request.Content = new StringContent(JsonConvert.SerializeObject(capacityUpdate), Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonConvert.DeserializeObject<Event>(content);
        }
        catch (Exception ex)
        {
            throw new JsonException(ex.Message);
        }
    }
}
