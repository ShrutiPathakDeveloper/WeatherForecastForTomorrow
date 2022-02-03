using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TemperatureForTomorrow.Controllers;

namespace TemperatureForTomorrow.Data
{
    /// <summary>
    /// Class to fetch values from weather api
    /// </summary>
    public class FetchData
    {
        private const string WeatherURL = "https://api.weather.gov/points/";
        
        private static bool ValidateCoordinates(double latitude, double longitude)
        {
            if (latitude < -90.0 || latitude > 90.0)
            {
                return false;
            }

            else if (longitude < -180.0 || longitude > 180.0)
            {
                return false;
            }

            return true;
        }

        //Calculates temerature median
        public async Task<WeatherForecastTomorrrow> GetTemperatureMedian(double latitude, double longitude)
        {
            var temperature = 0;
            var errorMessage = string.Empty;
            if(ValidateCoordinates(latitude, longitude))
            {
                var swedishCulture = new CultureInfo("sv-SE");
                swedishCulture.NumberFormat.NumberDecimalSeparator = "."; //avoid converting decimal to ,
                var path = WeatherURL + $"{latitude.ToString(swedishCulture)},{longitude.ToString(swedishCulture)}";
                var temeratures = new List<int>();
                try
                {
                    var jsonResponse = await GetJsonResponse(path);
                    if (jsonResponse == null)
                    {
                        errorMessage += "Response is null ";
                    }
                    else
                    {
                        string hourlyForecastUrl = JObject.Parse(jsonResponse["properties"].ToString())["forecastHourly"].ToString();
                        var hourlyforecast = await GetJsonResponse(hourlyForecastUrl);

                        var periods = JObject.Parse(hourlyforecast["properties"].ToString())["periods"].ToString();
                        var periodsForTomorrow = JsonConvert.DeserializeObject<List<Period>>(periods).Where(period => period.startTime.Date.Equals(DateTime.Now.Date.AddDays(1)) && period.endTime.Date.Equals(DateTime.Now.Date.AddDays(1))).ToList();
                        temeratures = periodsForTomorrow.OrderBy(period => period.temperature).Select(period => period.temperature).ToList();
                    }
                   
                }
                catch (Exception)
                {
                    errorMessage += "Error fetching data from weather api";
                }
                temperature = CalculateMedian(temeratures);
                     
            }
            else
            {
                errorMessage += "Coordinates are not valid ";
            }

            return new WeatherForecastTomorrrow()
            {
                MedianTemperatureForTomorrow = temperature,
                Latitude = latitude,
                Longitude = longitude,
                Error = errorMessage,
            };
        }      

        private static int CalculateMedian(List<int> temeratures)
        {
            //calculate median            
            if (temeratures.Count > 0)
            {
                if (temeratures.Count % 2 == 0)
                {
                    var midpoint = temeratures.Count / 2 - 1;
                    return (temeratures.ElementAt(midpoint) + temeratures.ElementAt(midpoint + 1)) / 2;
                }
                else
                {
                    var midpoint = (temeratures.Count - 1) / 2;
                    return temeratures.ElementAt(midpoint);
                }
            }
            return 0;
        }

        //get response from weather api
        private static async Task<JObject> GetJsonResponse(string path)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "(myweatherapp.com, contact@myweatherapp.com)");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            using HttpResponseMessage res = await client.GetAsync(path);
            using HttpContent content = res.Content;
            var response = await content.ReadAsStringAsync();
            if (response.Contains("404").Equals(true))
            {
                throw new Exception("No data found, wrong input ?");
            }
            else
            {
                return JObject.Parse(response);
            }
        }
    }
}
