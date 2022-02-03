using System;

namespace TemperatureForTomorrow.Data
{
    /// <summary>
    /// Class to deseralize extracted period values from json
    /// </summary>
    public class Period
    {
        public DateTime startTime;
        public DateTime endTime;
        public int temperature;
    }
}
