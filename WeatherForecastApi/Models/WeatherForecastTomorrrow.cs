namespace TemperatureForTomorrow.Controllers
{
    /// <summary>
    /// Weather forecast as output
    /// </summary>
    public class WeatherForecastTomorrrow
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int MedianTemperatureForTomorrow { get; set; }
        public string Error { get; set; }
    }
}
