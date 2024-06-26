﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for Hourly.xaml
    /// </summary>
    public partial class Hourly : UserControl
    {
        MainWindow mainWindow = new MainWindow();
        private const string filePath = "LastUpdated.json";
        private static HttpClient httpClient = new HttpClient();
        public  DateTime Date = DateTime.Today;

        public Hourly()
        {
            InitializeComponent();

        }
      
        public async Task hourlydata(string location, DateTime selectedDate, int currentCardIndex = 0)
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";

                bool isValidCity = await MainWindow.IsValidLocationAsync(location);

                if (!isValidCity)
                {
                    var coordinates = await MainWindow.GetCoordinatesFromGeoNamesAsync(location);

                    if (coordinates.HasValue)
                    {
                        string latitude = coordinates.Value.Latitude.ToString();
                        string longitude = coordinates.Value.Longitude.ToString();
                        url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&appid={apikey}";
                    }
                    else
                    {
                        throw new Exception("Invalid location and unable to retrieve coordinates.");
                    }
                }

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(jsonResponse);
                    WeatherParameters.root output = result;
                    int cardIndex = currentCardIndex;
                    var HourData = UpdatedLocation.LoadWeatherdata(filePath);
                    HourData.HourlyWeathers.Clear();
                    foreach (var item in output.list)
                    {
                        if (item.dt_txt.Date >= selectedDate.Date)
                        {
                            string message = string.Format("{0}", item.weather[0].description);
                            double Dewpoint = mainWindow.CalculateDewPoint(item.main.temp, item.main.humidity);
                            string DewpointString = $"{Dewpoint} mm";
                            string icon = item.weather[0].icon;

                            ImageSource iconImage = await MainWindow.GetImage(icon);

                            var hourlyweatherlist = new UpdatedLocation.HourlyWeather
                            {
                                Date = item.dt_txt.Date,
                                Temperature = item.main.temp,
                                WeatherType = message,
                                HourlyDewPoint = Dewpoint,
                                Time = item.dt_txt
                            };
                            cardIndex++;
                            if (cardIndex >= 10)
                            {
                                break;
                            }
                                
                                HourData.HourlyWeathers.Add(hourlyweatherlist);
                                UpdateWeatherCard(cardIndex, iconImage, Convert.ToInt32(item.main.temp), message,DewpointString, item.dt_txt);
                            
                        }
                    }
                    UpdatedLocation.SaveWeatherData(HourData, filePath);

                }
            }
            catch (Exception)
            {
                var hourlyWeather = UpdatedLocation.LoadWeatherdata(filePath);
                for (int i = 0; i < Math.Min(9, hourlyWeather.HourlyWeathers.Count); i++)
                {
                    var weather = hourlyWeather.HourlyWeathers[i];
                    UpdateWeatherCard(i, null, Convert.ToInt32(weather.Temperature), weather.WeatherType, "N/A", weather.Time);
                }
            }
        }
        public void UpdateWeatherCard(int cardIndex, ImageSource icon, int temperature, string description, string windspeed, DateTime dateTime)
        {
            string dateName = $"WeatherDate{cardIndex}";
            string iconName = $"Icon{cardIndex}";
            string maxTempName = $"MaxDayTemperature{cardIndex}";
            string descriptionName = $"WeatherType{cardIndex}";
            string windName = $"wind{cardIndex}";
            string timeName = $"Time{cardIndex}";

            var weatherDate = this.FindName(dateName) as TextBlock;
            var iconImage = this.FindName(iconName) as Image;
            var maxTemp = this.FindName(maxTempName) as TextBlock;
            var descriptionLabel = this.FindName(descriptionName) as TextBlock;
            var windText = this.FindName(windName) as TextBlock;
            var timeText = this.FindName(timeName) as TextBlock;

            if (weatherDate != null)
                weatherDate.Text = dateTime.ToString("dd MMM");

            if (maxTemp != null)
                maxTemp.Text = $"{temperature}°C";

            if (descriptionLabel != null)
                descriptionLabel.Text = description;

            if (windText != null)
                windText.Text = windspeed;

            if (timeText != null)
                timeText.Text = dateTime.ToString("hh:mm tt");

            if (iconImage != null)
            {
                iconImage.Source = icon;
            }
            else
            {
                MessageBox.Show($"Icon image {iconName} not found.");
            }
            this.UpdateLayout();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           await hourlydata(MainWindow.locationname, Date.Date);
        }
    }
}
   
  

