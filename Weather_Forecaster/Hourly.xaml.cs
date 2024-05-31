using Newtonsoft.Json;
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

        public Hourly()
        {
            InitializeComponent();

        }
      
        public async void hourlydata(string location, DateTime selectedDate)
        {
            try
            {
               
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";

                using (HttpClient httpClient = new HttpClient())
                {
                    bool isValidCity = await MainWindow.IsValidLocationAsync(location);

                    if (isValidCity)
                    {
                        // Use city name
                        url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                    }
                    else
                    {
                        // Assume location is in "latitude,longitude" format
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
                        int cardIndex = 1;

                        foreach (var item in output.list)
                        {
                            if (item.dt_txt.Date == selectedDate.Date)
                            {
                                string message = string.Format("{0}", item.weather[0].description);
                                double Dewpoint = mainWindow.CalculateDewPoint(item.main.temp, item.main.humidity);
                                double windspeed = item.wind.speed * 3.6;
                                double windDirectionDegree = item.wind.deg;
                                mainWindow.CalculateWindDirection(windDirectionDegree);
                                string icon = item.weather[0].icon;


                                // Call Imagedata to get the icon image
                                ImageSource iconImage = await mainWindow.GetImage(icon);

                                //var hourly = this.FindName("Hourly") as Hourly;

                                UpdateWeatherCard(cardIndex, iconImage, Convert.ToInt32(item.main.temp), message, windspeed.ToString("F2") + " " + windDirectionDegree, item.dt_txt.Date);

                                if (cardIndex > 9)
                                {
                                    break;
                                }
                                cardIndex++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        public void UpdateWeatherCard(int cardIndex, ImageSource icon, int temperature, string description, string windspeed, DateTime hour)
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
                weatherDate.Text = hour.ToString("dd MMM");

            if (maxTemp != null)
                maxTemp.Text = $"{temperature}°C";

            if (descriptionLabel != null)
                descriptionLabel.Text = description;

            if (windText != null)
                windText.Text = windspeed;

            if (timeText != null)
                timeText.Text = hour.ToString("t");

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            hourlydata(MainWindow.locationname, DateTime.Now.Date);
        }
    }
}
   
  

