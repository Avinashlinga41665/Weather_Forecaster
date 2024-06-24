using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using static Weather_Forecaster.UpdatedLocation;
using System.Runtime.InteropServices;

namespace Weather_Forecaster
{
    internal class UpdatedLocation //Avinash
    {
        public class WeatherDataContainer
        {
            public LastUpdated DailyWeather { get; set; }
            public List<WeeklyWeather> WeeklyWeather { get; set; } = new List<WeeklyWeather>();// Initialize the list
            public List<HourlyWeather> HourlyWeathers { get; set; } = new List<HourlyWeather>(); // Initialize the list
            public List<Summary> SummaryData { get; set; } = new List<Summary>(); // Initialize the list

            public WeatherInsights weatherInsights { get; set; }
            public  Sun SunData { get; set; }
            public Moon MoonData { get; set; }
            public SuggestionsForDay suggestionsForDay { get; set; }
            public PieChartData pieChartData { get; set; }
           
        }
        public class LastUpdated
        {
            public string Location { get; set; }
            public DateTime Time { get; set; }
            public int Temperature { get; set; }
            public string WeatherType { get; set; }
            public string WeatherDescription { get; set; }
            public string AirQuality { get; set; }
            public int Wind { get; set; }
            public int Humidity { get; set; }
            public int Visibility { get; set; }
            public double Pressure { get; set; }
            public double DewPoint { get; set; }
            public DateTime SummaryTime { get; set; }
            
        }
        public class Summary
        {
         public DateTime SummarDate {  get; set; }
         public int SummaryTemperature { get; set; }
        public int SummaryRainPercentage { get; set; }

        }
        public class WeatherInsights
        {
            public string WeatherInsightsWeatherDescription1 { get; set; }
            public string WeatherInsightsWeatherDescription2 { get; set; }
            public int WeatherInsightsTemperature1 { get; set; }
            public int WeatherInsightsTemperature2 { get; set; }
            public int AvgHighTemp { get; set; }
            public int AvgLowTemp { get; set; }

        }
        public class Sun
        { 
            public string SunTime1 { get; set; }
            public string SunTime2 { get; set; }
        }
        public class Moon
        {
            public string MoonTime1 { get; set; }
            public string MoonTime2 { get; set; }
        }
        public class SuggestionsForDay
        {
            public string Umberella { get; set; }
            public string Outdoor { get; set; }
            public string Driving { get; set; }
            public string Clothing { get; set; }
        }
        public class PieChartData
        {      
            public int PiechartWeatherType1 { get; set; }
            public int PiechartWeatherType2 { get; set; }
        }

        public class WeeklyWeather
        {
         public double  MinTemperature { get; set; }
         public double  MaxTemperature { get; set;}
         public double WeeklyDewpoint { get; set;} 
            public string Description { get; set; }
        }
        public class HourlyWeather
        {
            public DateTime Date { get; set; }
            public double Temperature { get; set; }
            public string WeatherType { get; set; }
            public double HourlyDewPoint { get; set; }
            public DateTime Time { get; set; }
            
        }

        public static void SaveWeatherData (WeatherDataContainer data, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented); File.WriteAllText(filePath, jsonString);
        }
        public static WeatherDataContainer LoadWeatherdata(string filePath)
        {
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<WeatherDataContainer>(jsonString);
            }
            return new WeatherDataContainer();
        }
    }
}
