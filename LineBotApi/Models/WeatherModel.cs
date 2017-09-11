using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineBotApi.Models
{
    public class WeatherModel
    {
        public class clsWeather
        {
            public string publicTime;
            public string title;
            public clsDescription description;
            public string link;
            public IList<clsForecasts> forecasts;
            public clsLocation location;
            public IList<clsPinpointLocations> pinpointLocations;
        }
        public class clsDescription
        {
            public string text;
            public string publicTime;
        }
        public class clsForecasts
        {
            public string dateLabel;
            public string telop;
            public string date;
            public clsTemperature temperature;
            public clsImage image;
        }
        public class clsTemperature
        {
            public clsTemperatureDetail min;
            public clsTemperatureDetail max;
        }
        public class clsTemperatureDetail
        {
            public string celsius;
            public string fahrenheit;
        }
        public class clsImage
        {
            public int width;
            public int height;
            public string url;
            public string title;
        }
        public class clsLocation
        {
            public string city;
            public string area;
            public string prefecture;
        }
        public class clsPinpointLocations
        {
            public string link;
            public string name;
        }
    }
}