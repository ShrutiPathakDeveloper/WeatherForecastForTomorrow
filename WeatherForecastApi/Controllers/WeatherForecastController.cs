using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using TemperatureForTomorrow.Controllers;
using TemperatureForTomorrow.Data;

namespace WeatherForecast.Controllers
{
    /// <summary>
    /// Controller for handling weather forecast.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        /// <summary>
        /// Get median value for all values for tomorrow.
        /// </summary>
        /// <param name="locationCoordinates">Location coordinates for all locations</param>
        /// <returns>Median Temperature for all locations</returns>
        [HttpPost]
        [Route("GetTemperatureForTomorrow")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WeatherForecastTomorrrow>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<WeatherForecastTomorrrow>>> GetTemperatureForTomorrow([FromBody] List<LocationCoordinate> locationCoordinates)
        {
            if (locationCoordinates == null || locationCoordinates.Count == 0)
            {
                return BadRequest();
            }

            var fetchData = new FetchData();
            var outputs = new List<WeatherForecastTomorrrow>();

            foreach (var locationCoordinate in locationCoordinates)
            {
                outputs.Add(await fetchData.GetTemperatureMedian(locationCoordinate.Latitude, locationCoordinate.Longitude));
            }
            return Ok(outputs);
        }

        /// <summary>
        /// Get median value for all values for tomorrow parallelly.
        /// </summary>
        /// <param name="locationCoordinates">Location coordinates for all locations</param>
        /// <returns>Median Temperature for all locations</returns>
        [HttpPost]
        [Route("GetTemperatureForTomorrowParallel")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WeatherForecastTomorrrow>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<WeatherForecastTomorrrow>>> GetTemperatureForTomorrowParallel([FromBody] List<LocationCoordinate> locationCoordinates)
        {
            if (locationCoordinates == null || locationCoordinates.Count == 0)
            {
                return BadRequest();
            }
            List<Task<WeatherForecastTomorrrow>> tasks = new();
            var fetchData = new FetchData();
            var outputs = new List<WeatherForecastTomorrrow>();

            foreach (var locationCoordinate in locationCoordinates)
            {
                tasks.Add(Task.Run(async () => await fetchData.GetTemperatureMedian(locationCoordinate.Latitude, locationCoordinate.Longitude)));

            }
            var results = await Task.WhenAll(tasks);
            foreach (var task in results)
            {
                outputs.Add(task);
            }

            return Ok(outputs);
        }
    }


   

}
