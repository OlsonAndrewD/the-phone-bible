using Garnet.Domain.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Garnet.Services
{
    public class TimeService : ITimeService
    {
        private readonly string _apiKey;

        public TimeService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<TimeSpan?> GetLocalTimeAsync(string city, string state, string zip)
        {
            var components = new List<KeyValuePair<string, string>>();
            if (!string.IsNullOrEmpty(city))
            {
                components.Add(new KeyValuePair<string, string>("locality", city));
            }
            if (!string.IsNullOrEmpty(state))
            {
                components.Add(new KeyValuePair<string, string>("administrative_area", state));
            }
            if (!string.IsNullOrEmpty(zip))
            {
                components.Add(new KeyValuePair<string, string>("postal_code", zip));
            }

            if (components.Count == 0)
            {
                return null;
            }

            var client = CreateGoogleMapsRestClient();
            var request = new RestRequest("geocode/json");
            request.AddParameter("components", string.Join("|", components.Select(x =>
                string.Join(":", x.Key, x.Value))));
            dynamic response = JsonConvert.DeserializeObject((await client.ExecuteGetTaskAsync(request)).Content);
            if (response.status == "OK")
            {
                dynamic location = response.results[0].geometry.location;

                request = new RestRequest("timezone/json");
                request.AddParameter("location", string.Join(",", location.lat, location.lng));
                request.AddParameter("timestamp", (int)((DateTime.UtcNow - GoogleTimestampOrigin).TotalSeconds));
                response = JsonConvert.DeserializeObject((await client.ExecuteGetTaskAsync(request)).Content);
                if (response.status == "OK")
                {
                    double rawOffset = response.rawOffset;
                    double dstOffset = response.dstOffset;

                    return DateTime.UtcNow.AddSeconds(rawOffset).AddSeconds(dstOffset).TimeOfDay;
                }
            }

            return null;
        }

        private RestClient CreateGoogleMapsRestClient()
        {
            var restClient = new RestClient("https://maps.googleapis.com/maps/api");
            restClient.AddDefaultParameter("key", _apiKey);
            return restClient;
        }

        private static readonly DateTime GoogleTimestampOrigin = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
