using System;
using System.Collections.Generic;
using System.Linq;
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
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;
using System.Net.Http;
namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for MoreDetails.xaml
    /// </summary>
    public partial class MoreDetails : UserControl
    {

        public MoreDetails()
        {
            InitializeComponent();

        }
        private PieChart GenerateWeeklyDonutChart(int sunnyCloudyClearCount, int rainySnowyCount, string title)
        {
            try
            {
                // Create a new PieChart
                PieChart chart = new PieChart();

                // Add series to the chart
                chart.Series = new SeriesCollection
                {
                   new PieSeries
                   {
                       Title = "Sunny, Cloudy, Clear",
                       Values = new ChartValues<int> { sunnyCloudyClearCount },
                       Fill = System.Windows.Media.Brushes.Orange
                   },
                   new PieSeries
                   {
                       Title = "Rainy, Snowy",
                       Values = new ChartValues<int> { rainySnowyCount },
                       Fill = System.Windows.Media.Brushes.Blue
                   }
                };
                chart.DisableAnimations = true;
                // Configure legend
                chart.LegendLocation = LegendLocation.Right;
                // Customize chart appearance
                chart.Background = Brushes.Transparent; // Set background color to transparent
                chart.Foreground = Brushes.White; // Set foreground color for text elements

                return chart;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        public async void Weeklydata(string location)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
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
                    var json = web.DownloadString(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;

                    // Count the number of "sunny, cloudy, clear" days and "rainy, snowy" days
                    int sunnyCloudyClearCount = 0;
                    int rainySnowyCount = 0;

                    foreach (var item in output.list)
                    {
                        if (item.weather[0].main.Contains("Clouds") || item.weather[0].main.Contains("Clear") || item.weather[0].main.Contains("Sunny"))
                            sunnyCloudyClearCount++;
                        else if (item.weather[0].main.Contains("Rain") || item.weather[0].main.Contains("Snow"))
                            rainySnowyCount++;
                    }        
                    SunnyCloudyHours.Text = sunnyCloudyClearCount.ToString();
                    RainSnowHours.Text = rainySnowyCount.ToString();
                    PieChartData.Content = GenerateWeeklyDonutChart(sunnyCloudyClearCount, rainySnowyCount, "Weekly Weather Forecast");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving Weekly Weather details: " + ex.Message);
            }
        }
        public async void sunsetsrises(string location)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                    bool isValidCity = await MainWindow.IsValidLocationAsync(location);
                    if (isValidCity)
                    {
                        // Use city name
                        url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                    }
                    else
                    {
                        // Assume location is in "latitude,longitude" format
                        var coordinates = await MainWindow.GetCoordinatesFromGeoNamesAsync(location);

                        if (coordinates.HasValue)
                        {
                            string latitudes = coordinates.Value.Latitude.ToString();
                            string longitudes = coordinates.Value.Longitude.ToString();
                            url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitudes}&lon={longitudes}&units=metric&appid={apikey}";
                        }
                        else
                        {
                            throw new Exception("Invalid location and unable to retrieve coordinates.");
                        }
                    }
                    var json = web.DownloadString(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;
                    double latitude = output.coord.lat;
                    double longitude = output.coord.lon;

                    int sunrises = output.sys.sunrise;
                    int sunsets = output.sys.sunset;

                    DateTime sunriseUtc = UnixTimeStampToDateTime(sunrises);
                    DateTime sunsetUtc = UnixTimeStampToDateTime(sunsets);
                    TimeSpan difference = sunsetUtc - sunriseUtc;

                    Sunrise.Text = sunriseUtc.ToString("h:mm tt"); // Display sunrise time with AM/PM indication
                    Sunset.Text = sunsetUtc.ToString("h:mm tt"); // Display sunset time with AM/PM indication
                    int totalHours = (int)difference.TotalHours;
                    int totalMinutes = difference.Minutes;

                    // Create a formatted string
                    string diffText = $"{totalHours} hr {totalMinutes} min";

                    // Set the label text
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            try
            {
                // Unix timestamp is seconds past epoch
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                return dtDateTime;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
                return DateTime.Today;
            }
        }
        private async Task GetMoonTimes(string date, string location)
        {
            string apiKey = "7dec88a780f64b06959170935242605"; // Replace with your WeatherAPI key
            string baseUrl = "http://api.weatherapi.com/v1/astronomy.json";
            string url = $"{baseUrl}?key={apiKey}&q={location}&dt={date}";

            using (HttpClient client = new HttpClient())
            {
                try
                {

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(responseBody);

                    string moonrise = json["astronomy"]["astro"]["moonrise"].ToString();
                    string moonset = json["astronomy"]["astro"]["moonset"].ToString();

                    // Update the labels on the form
                    Moonrise.Text = moonrise;
                    Moonset.Text = moonset;
                }
                catch (HttpRequestException e)
                {
                    MessageBox.Show("Request error: " + e.Message);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: " + e.Message);
                }
            }
        }
        public void Suggestionsforday(string location)
        {

            using (WebClient web = new WebClient())
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                var json = web.DownloadString(url);
                var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                WeatherParameters.root output = result;
                foreach (var item in output.list)
                {
                    if (item.dt_txt.Date == DateTime.Today.Date)
                    {
                        string weatherdescription = string.Format("{0}", item.weather[0].description);
                        double visiblity = item.visibility / 1000.0;
                        double temperature = item.main.temp;
                        double windspeed = item.wind.speed * 3.6;
                        double feelslike = item.main.feels_like;
                        double humidity = item.main.humidity;
                        string umbrellatext = UmbrellaMessage(weatherdescription);
                        string outdoorstext = OutdoorsMessage(weatherdescription, temperature, feelslike, windspeed);
                        string clothingtext = ClothingMessage(weatherdescription, temperature, humidity, windspeed);
                        string drivingtext = DrivingConditionsMessage(visiblity, temperature, windspeed, weatherdescription);
                        UmbrellaText.Text = umbrellatext;
                        OutdoorsText.Text = outdoorstext;
                        ClothingText.Text = clothingtext;
                        DrivingText.Text = drivingtext;
                       coloringSuggestionForDay(umbrellatext, outdoorstext, drivingtext, clothingtext);

                    }
                }
            }
        }
        public static string UmbrellaMessage(string weatherDescription)
        {
            switch (weatherDescription.ToLower())
            {
                case "clear":
                case "clear sky":
                    return "No need";
                case "fog":
                case "snow":
                case "few clouds":
                    return "Likely no needed";
                case "scattered clouds":
                case "broken clouds":
                case "overcast clouds":
                case "mist":
                case "haze":
                    return "Likely needed";
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    return "Need";
                case "thunderstorm with light rain":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                case "heavy rain":
                case "thunderstorm":
                    return "Must";
                default:
                    return "uncertain";
            }
        }
        public static string DrivingConditionsMessage(double visibility, double temperature, double windSpeed, string weatherDescription)
        {
            // Define thresholds for each factor
            const double VisibilityPoorThreshold = 5.0; // km
            const double VisibilityFairThreshold = 10.0; // km
            const double VisibilityGoodThreshold = 15.0; // km
            const double VisibilityGreatThreshold = 20.0; // km
            const double TemperaturePoorThreshold = 45.0; // degrees Celsius
            const double TemperatureFairThreshold = 35.0; // degrees Celsius
            const double TemperatureGoodThreshold = 25.0; // degrees Celsius
            const double TemperatureGreatThreshold = 15.0; // degrees Celsius
            const double WindSpeedPoorThreshold = 30.0; // km/h
            const double WindSpeedFairThreshold = 20.0; // km/h
            const double WindSpeedGoodThreshold = 15.0; // km/h
            const double WindSpeedGreatThreshold = 10.0; // km/h

            // Determine the driving conditions category based on the factors
            switch (weatherDescription.ToLower())
            {
                case "clear":
                case "clear sky":
                    if (visibility > VisibilityGreatThreshold && temperature < TemperatureGreatThreshold && windSpeed < WindSpeedGreatThreshold)
                        return "Great";
                    else if (visibility > VisibilityGoodThreshold && temperature < TemperatureGoodThreshold && windSpeed < WindSpeedGoodThreshold)
                        return "Good";
                    else if (visibility > VisibilityFairThreshold && temperature < TemperatureFairThreshold && windSpeed < WindSpeedFairThreshold)
                        return "Fair";
                    else
                        return "Poor";
                case "fog":
                case "snow":
                case "few clouds":
                    if (visibility < VisibilityPoorThreshold || temperature > TemperaturePoorThreshold || windSpeed > WindSpeedPoorThreshold)
                        return "Very poor";
                    else if (visibility < VisibilityFairThreshold || temperature > TemperatureFairThreshold || windSpeed > WindSpeedFairThreshold)
                        return "Poor";
                    else if (visibility < VisibilityGoodThreshold || temperature > TemperatureGoodThreshold || windSpeed > WindSpeedGoodThreshold)
                        return "Fair";
                    else if (visibility < VisibilityGreatThreshold || temperature > TemperatureGreatThreshold || windSpeed > WindSpeedGreatThreshold)
                        return "Good";
                    else
                        return "Great";
                case "scattered clouds":
                case "broken clouds":
                case "overcast clouds":
                case "mist":
                case "haze":
                    if (temperature > TemperaturePoorThreshold || windSpeed > WindSpeedPoorThreshold)
                        return "Very poor";
                    else if (temperature > TemperatureFairThreshold || windSpeed > WindSpeedFairThreshold)
                        return "Poor";
                    else if (temperature > TemperatureGoodThreshold || windSpeed > WindSpeedGoodThreshold)
                        return "Fair";
                    else
                        return "Good";
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    if (windSpeed > WindSpeedPoorThreshold)
                        return "Very poor";
                    else if (windSpeed > WindSpeedFairThreshold)
                        return "Poor";
                    else
                        return "Fair";
                case "thunderstorm with light rain":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                case "heavy rain":
                case "thunderstorm":
                    return "Very poor";
                default:
                    return "Uncertain";
            }
        }

        public static string OutdoorsMessage(string weatherDescription, double temperature, double feelsLike, double windSpeed)
        {

            const double TemperatureGoodThreshold = 20.0; // degrees Celsius
            const double TemperatureFairThreshold = 15.0; // degrees Celsius
            const double TemperaturePoorThreshold = 10.0; // degrees Celsius

            const double FeelsLikeGreatThreshold = 25.0; // degrees Celsius
            const double FeelsLikeGoodThreshold = 20.0; // degrees Celsius
            const double FeelsLikeFairThreshold = 15.0; // degrees Celsius
            const double FeelsLikePoorThreshold = 10.0; // degrees Celsius

            const double WindSpeedGreatThreshold = 10.0; // km/h
            const double WindSpeedGoodThreshold = 15.0; // km/h
            const double WindSpeedFairThreshold = 20.0; // km/h
            const double WindSpeedPoorThreshold = 25.0; // km/h
            const double WindSpeedVeryPoorThreshold = 30.0; // km/h

            switch (weatherDescription.ToLower())
            {
                case "clear":
                case "clear sky":
                    if (temperature > TemperatureGoodThreshold && windSpeed < WindSpeedGoodThreshold)
                    {
                        if (feelsLike > FeelsLikeGoodThreshold && feelsLike < FeelsLikeGreatThreshold)
                            return "Great"; // Perfect weather for outdoor activities
                        else if (feelsLike > FeelsLikeFairThreshold && feelsLike < FeelsLikeGoodThreshold)
                            return "Good"; // Good weather for outdoor activities
                    }
                    else if ((temperature < TemperaturePoorThreshold || temperature > TemperatureGoodThreshold) || (feelsLike < FeelsLikePoorThreshold || feelsLike > FeelsLikeGreatThreshold))
                    {
                        return "Poor";
                    }
                    else if ((temperature < 5 || temperature > 40) ||
                              (feelsLike < 5 || feelsLike > 40))// Clear skies but temperature and feels-like temperature are not ideal
                    {
                        return "Very poor";
                    }
                    return "Fair"; // Clear skies but temperature or wind speed not ideal
                case "cloudy":
                case "partly cloudy":
                case "mostly cloudy":
                    if (temperature <= TemperatureFairThreshold || windSpeed >= WindSpeedFairThreshold)
                        return "Poor"; // Cloudy with poor temperature or wind speed
                    else if (temperature <= TemperaturePoorThreshold || windSpeed >= WindSpeedGoodThreshold)
                        return "Very poor"; // Cloudy with very poor temperature or wind speed
                    else if (temperature <= TemperatureGoodThreshold || windSpeed >= WindSpeedGreatThreshold)
                        return "Fair"; // Cloudy but still fair for outdoor activities
                    else if ((temperature < TemperaturePoorThreshold || temperature > TemperatureGoodThreshold) || (feelsLike < FeelsLikePoorThreshold || feelsLike > FeelsLikeGreatThreshold))
                    {
                        return "Poor";
                    }
                    else if ((temperature < 5 || temperature > 40) ||
                              (feelsLike < 5 || feelsLike > 40))
                    {
                        return "Very poor";
                    }
                    else
                        return "Good"; // Cloudy with good temperature and wind speed
                case "rain":
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    if (windSpeed >= WindSpeedPoorThreshold)
                        return "Very poor"; // Rainy weather with poor wind speed
                    else if (temperature <= TemperaturePoorThreshold || feelsLike <= FeelsLikePoorThreshold)
                        return "Poor"; // Rainy weather with poor temperature or feels-like temperature
                    else
                        return "Fair"; // Rainy but still fair for outdoor activities
                case "thunderstorm":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                    return "Very poor"; // Thunderstorms, unsafe for outdoor activities
                case "snow":
                    if (windSpeed >= WindSpeedGoodThreshold)
                        return "Poor"; // Snowing with poor wind speed
                    else
                        return "Good"; // Snowing, suitable for winter activities
                case "fog":
                    if (temperature <= TemperaturePoorThreshold || feelsLike <= FeelsLikePoorThreshold || windSpeed >= WindSpeedVeryPoorThreshold)
                        return "Very poor"; // Foggy weather with poor temperature, feels-like temperature, or wind speed
                    else
                        return "Poor"; // Foggy, visibility might be reduced
                default:
                    return "Uncertain"; // Unknown weather condition
            }

        }
        public static string ClothingMessage(string weatherDescription, double temperature, double humidity, double windSpeed)
        {

            const double TemperatureGoodThreshold = 20.0; // degrees Celsius
            const double TemperatureFairThreshold = 15.0; // degrees Celsius
            const double TemperaturePoorThreshold = 10.0; // degrees Celsius

            const double HumidityHighThreshold = 70.0; // percentage
            const double HumidityModerateThreshold = 50.0; // percentage
            const double HumidityLowThreshold = 30.0; // percentage

            const double WindSpeedGreatThreshold = 10.0; // km/h
            const double WindSpeedGoodThreshold = 15.0; // km/h
            const double WindSpeedFairThreshold = 20.0; // km/h
            const double WindSpeedPoorThreshold = 25.0; // km/h
            const double WindSpeedVeryPoorThreshold = 30.0; // km/h

            switch (weatherDescription.ToLower())
            {
                case "clear":
                case "clear sky":
                    if (temperature > TemperatureGoodThreshold && windSpeed < WindSpeedGoodThreshold)
                    {
                        if (humidity < HumidityLowThreshold)
                            return "Light jacket"; // Low humidity, clear skies, and light wind call for a light jacket
                        else if (humidity >= HumidityLowThreshold && humidity < HumidityModerateThreshold)
                            return "Long sleeves"; // Moderate humidity, clear skies, and light wind call for long sleeves
                        else
                            return "T-shirt"; // High humidity, clear skies, and light wind call for a T-shirt
                    }
                    else if ((temperature < TemperaturePoorThreshold || temperature > TemperatureGoodThreshold) ||
                             humidity >= HumidityHighThreshold)
                    {
                        return "Shorts"; // Temperature not ideal for lighter clothing or high humidity
                    }
                    return "Long sleeves"; // Clear skies but wind speed not ideal for lighter clothing
                case "cloudy":
                case "partly cloudy":
                case "mostly cloudy":
                    if ((temperature <= TemperatureFairThreshold || windSpeed >= WindSpeedFairThreshold) ||
                        (temperature < TemperaturePoorThreshold || temperature > TemperatureGoodThreshold) ||
                        humidity >= HumidityHighThreshold || temperature <= TemperatureGoodThreshold || windSpeed >= WindSpeedGreatThreshold)
                    {
                        return "Long sleeves"; // Cloudy with poor temperature or wind speed, or high humidity
                    }
                    else
                        return "Light jacket"; // Cloudy with good temperature and wind speed
                case "rain":
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    if (windSpeed >= WindSpeedPoorThreshold)
                        return "Long sleeves"; // Rainy weather with poor wind speed
                    else if (temperature < TemperaturePoorThreshold || humidity >= HumidityModerateThreshold)
                        return "Light jacket"; // Rainy weather with poor temperature or high humidity
                    else
                        return "Long sleeves"; // Rainy but still fair for breathable clothing
                case "thunderstorm":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                    return "Heavy coat"; // Thunderstorms, necessitating heavy coat
                case "snow":
                    if (windSpeed >= WindSpeedGoodThreshold)
                        return "Heavy coat"; // Snowing with poor wind speed
                    else
                        return "Long sleeves"; // Snowing, suitable for long sleeves
                case "fog":
                    if (temperature < TemperaturePoorThreshold || humidity >= HumidityModerateThreshold || windSpeed >= WindSpeedVeryPoorThreshold)
                        return "Heavy coat"; // Foggy weather with poor temperature, high humidity, or poor wind speed
                    else
                        return "Long sleeves"; // Foggy but still fair for breathable clothing
                default:
                    return "Uncertain"; // Unknown weather condition
            }

        }
        public void coloringSuggestionForDay(string umbrellamessage,string outdoorsmessage,string drivingconditionsmessage,string clothingmessage)
        {
            switch (umbrellamessage.ToLower())
            {
                case "no need":
                    UmbrellaColor.Fill = Brushes.Green;
                    break;
                case "likely no need":
                        UmbrellaColor.Fill = Brushes.Green;                  
                    break;
                case "likely needed":
                        UmbrellaColor.Fill = Brushes.Yellow;                   
                    break;
                case "need":
                        UmbrellaColor.Fill = Brushes.Orange;                    
                    break;
                case "must":
                        UmbrellaColor.Fill = Brushes.Red;
                    break;
                default:
                    UmbrellaColor.Fill = new SolidColorBrush(Colors.White);
                    break;
            }
            switch (outdoorsmessage.ToLower())
            {
                case "great":
                case "good":
                    OutdoorsColor.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case "fair":
                    OutdoorsColor.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case "poor":
                    OutdoorsColor.Fill = new SolidColorBrush(Colors.Orange);
                    break;
                case "very poor":
                    OutdoorsColor.Fill = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    OutdoorsColor.Fill = new SolidColorBrush(Colors.White); 
                    break;
            }
            switch (clothingmessage.ToLower())
            {
                case "long sleeves":
                    ClothingColor.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case "breathable clothing":
                    ClothingColor.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case "shorts":
                case "heavy coat":
                    ClothingColor.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case "light jacket":
                case "t-shirt":
                    ClothingColor.Fill = new SolidColorBrush(Colors.Lavender);
                    break;
                default:
                    ClothingColor.Fill =  new SolidColorBrush(Colors.White);
                    break;
            }

            switch (drivingconditionsmessage.ToLower())
            {
                case "Great":
                case "Good":
                    DrivingConditionsColor.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case "fair":
                    DrivingConditionsColor.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case "poor":
                    DrivingConditionsColor.Fill = new SolidColorBrush(Colors.Orange);
                    break;
                case "very poor":
                    DrivingConditionsColor.Fill = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    DrivingConditionsColor.Fill = new SolidColorBrush(Colors.White);
                    break;
            }

        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Weeklydata(MainWindow.locationname);
            sunsetsrises(MainWindow.locationname);
            Suggestionsforday(MainWindow.locationname);
            await  GetMoonTimes(DateTime.Today.Date.ToString(), MainWindow.locationname);
        }
    }
}
