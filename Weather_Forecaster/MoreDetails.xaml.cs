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
using static Weather_Forecaster.UpdatedLocation;
using System.Runtime.Remoting.Messaging;
namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for MoreDetails.xaml
    /// </summary>
    public partial class MoreDetails : UserControl
    {
        public DateTime Date = DateTime.Today;
        private const string filePath = "LastUpdated.json";
        private static HttpClient httpClient = new HttpClient();

        public MoreDetails()
        {
            InitializeComponent();

        }
        public PieChart GenerateWeeklyDonutChart(int sunnyCloudyClearCount, int rainySnowyCount, string title)
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
        public async Task Weeklydata(string location)
        {
            try
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
                    var json = await httpClient.GetStringAsync(url);
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
                    var pieChartData = new PieChartData
                    {
                        PiechartWeatherType1 = sunnyCloudyClearCount,
                        PiechartWeatherType2 = rainySnowyCount,

                    };
                    var weatherdata = LoadWeatherdata(filePath);
                    weatherdata.pieChartData = pieChartData;
                    SaveWeatherData(weatherdata, filePath);
                
            }
            catch (Exception)
            {
                var moreDetails = LoadWeatherdata(filePath);
                if (moreDetails.pieChartData != null)
                {
                    // Update UI elements with last saved data
                    Dispatcher.Invoke(() =>
                    {
                        SunnyCloudyHours.Text = moreDetails.pieChartData.PiechartWeatherType1.ToString();
                        RainSnowHours.Text = moreDetails.pieChartData.PiechartWeatherType2.ToString();
                        PieChartData.Content = GenerateWeeklyDonutChart(moreDetails.pieChartData.PiechartWeatherType1, moreDetails.pieChartData.PiechartWeatherType2, "Weekly Weather Forecast");

                    });
                }
            }
        }
      
        public async Task sunsetsrises(string location , DateTime Date)
        {
            try
            {
                string apiKey = "7dec88a780f64b06959170935242605"; // Replace with your WeatherAPI key
                string baseUrl = "http://api.weatherapi.com/v1/astronomy.json";
                string url = $"{baseUrl}?key={apiKey}&q={location}&dt={Date.Date}";

                using (HttpClient client = new HttpClient())
                {
                    

                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(responseBody);

                        string Sunrises = json["astronomy"]["astro"]["sunrise"].ToString();
                        string Sunsets = json["astronomy"]["astro"]["sunset"].ToString();

                        // Update the labels on the form
                        Sunrise.Text = Sunrises;
                        Sunset.Text = Sunsets;
                        var sunData = new Sun
                        {
                            SunTime1 = Sunrises,
                            SunTime2 = Sunsets,

                        };
                        var weatherdata = LoadWeatherdata(filePath);
                        weatherdata.SunData = sunData;
                        SaveWeatherData(weatherdata, filePath);
                    

                   
                }
            }
            catch (Exception)
            {
                var moreDetails = LoadWeatherdata(filePath);
                if (moreDetails.SunData != null)
                {
                    // Update UI elements with last saved data
                    Dispatcher.Invoke(() =>
                    {
                        Sunrise.Text = moreDetails.SunData.SunTime1.ToString();
                        Sunset.Text = moreDetails.SunData.SunTime2.ToString();

                    });
                }
            }
        }
        public DateTime UnixTimeStampToDateTime(double unixTimeStamp)
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
        public async Task GetMoonTimes(string date, string location)
        {
            string apiKey = "7dec88a780f64b06959170935242605"; // Replace with your WeatherAPI key
            string baseUrl = "http://api.weatherapi.com/v1/astronomy.json";
            string url = $"{baseUrl}?key={apiKey}&q={location}&dt={date}";


                try
                {

                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(responseBody);

                    string moonrise = json["astronomy"]["astro"]["moonrise"].ToString();
                    string moonset = json["astronomy"]["astro"]["moonset"].ToString();

                    // Update the labels on the form
                    Moonrise.Text = moonrise;
                    Moonset.Text = moonset;
                    var moonData = new Moon
                    {
                        MoonTime1 = moonrise,
                        MoonTime2 = moonset,

                    };
                    var weatherdata = LoadWeatherdata(filePath);
                    weatherdata.MoonData = moonData;
                    SaveWeatherData(weatherdata, filePath);
                }
                catch (Exception)
                {
                    var moreDetails = LoadWeatherdata(filePath);
                    if (moreDetails.MoonData != null)
                    {
                        // Update UI elements with last saved data
                        Dispatcher.Invoke(() =>
                        {                           
                            Moonrise.Text = moreDetails.MoonData.MoonTime1.ToString();
                            Moonset.Text = moreDetails.MoonData.MoonTime2.ToString();

                        });
                    }
                }
            
        }
        public async Task Suggestionsforday(string location, DateTime selectedDate)
        {
            try
            {

                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                    var json =  await httpClient.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;
                    foreach (var item in output.list)
                    {
                        if (item.dt_txt.Date == selectedDate.Date)
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

                            var suggestionsForDay = new SuggestionsForDay
                            {                                                              
                                Umberella = umbrellatext,
                                Outdoor = outdoorstext,
                                Driving = drivingtext,
                                Clothing = clothingtext,

                            };
                            var weatherdata = LoadWeatherdata(filePath);
                            weatherdata.suggestionsForDay= suggestionsForDay;
                            SaveWeatherData(weatherdata, filePath);

                        }
                    }
                
            }
            catch(Exception)
            {
                var moreDetails = LoadWeatherdata(filePath);
                if (moreDetails.suggestionsForDay != null)
                {
                    // Update UI elements with last saved data
                    Dispatcher.Invoke(() =>
                    {
                        UmbrellaText.Text = moreDetails.suggestionsForDay.Umberella.ToString();
                        OutdoorsText.Text = moreDetails.suggestionsForDay.Outdoor.ToString();
                        ClothingText.Text = moreDetails.suggestionsForDay.Clothing.ToString();
                        DrivingText.Text = moreDetails.suggestionsForDay.Driving.ToString();
                        coloringSuggestionForDay(moreDetails.suggestionsForDay.Umberella.ToString(), moreDetails.suggestionsForDay.Outdoor.ToString(), moreDetails.suggestionsForDay.Driving.ToString(), moreDetails.suggestionsForDay.Clothing.ToString());

                    });
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
            const double TemperatureGreatThreshold = 25.0; // degrees Celsius
            const double TemperatureGoodThreshold = 20.0; // degrees Celsius
            const double TemperatureFairThreshold = 15.0; // degrees Celsius
            const double TemperaturePoorThreshold = 10.0; // degrees Celsius

            const double FeelsLikeGreatThreshold = 30.0; // degrees Celsius
            const double FeelsLikeGoodThreshold = 25.0; // degrees Celsius
            const double FeelsLikeFairThreshold = 20.0; // degrees Celsius
            const double FeelsLikePoorThreshold = 15.0; // degrees Celsius

            const double WindSpeedGreatThreshold = 10.0; // km/h
            const double WindSpeedGoodThreshold = 15.0; // km/h
            const double WindSpeedFairThreshold = 20.0; // km/h
            const double WindSpeedPoorThreshold = 25.0; // km/h
            const double WindSpeedVeryPoorThreshold = 30.0; // km/h

            switch (weatherDescription.ToLower())
            {
                case "clear":
                case "clear sky":
                    if (temperature > TemperatureGreatThreshold && feelsLike > FeelsLikeGreatThreshold && windSpeed < WindSpeedGoodThreshold)
                        return "Great";
                    else if ((temperature > TemperatureGoodThreshold && temperature <= TemperatureGreatThreshold) || (feelsLike > FeelsLikeGoodThreshold && feelsLike <= FeelsLikeGreatThreshold))
                        return "Good";
                    else if ((temperature > TemperatureFairThreshold && temperature <= TemperatureGoodThreshold) || (feelsLike > FeelsLikeFairThreshold && feelsLike <= FeelsLikeGoodThreshold))
                        return "Fair";
                    else if ((temperature > TemperaturePoorThreshold && temperature <= TemperatureFairThreshold) || (feelsLike > FeelsLikePoorThreshold && feelsLike <= FeelsLikeFairThreshold))
                        return "Poor";
                    else
                        return "Very poor";
                case "cloudy":
                case "partly cloudy":
                case "mostly cloudy":
                case "few clouds": 
                case "overcast clouds":
                    if (temperature <= TemperaturePoorThreshold || feelsLike <= FeelsLikePoorThreshold || windSpeed >= WindSpeedFairThreshold)
                        return "Poor";
                    else if ((temperature <= TemperatureFairThreshold && temperature > TemperaturePoorThreshold) || (feelsLike <= FeelsLikeFairThreshold && feelsLike > FeelsLikePoorThreshold) || windSpeed >= WindSpeedGoodThreshold)
                        return "Fair";
                    else if ((temperature <= TemperatureGoodThreshold && temperature > TemperatureFairThreshold) || (feelsLike <= FeelsLikeGoodThreshold && feelsLike > FeelsLikeFairThreshold) || windSpeed >= WindSpeedGreatThreshold)
                        return "Good";
                    else
                        return "Great";
                case "rain":
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    if (windSpeed >= WindSpeedPoorThreshold)
                        return "Poor";
                    else if (temperature <= TemperaturePoorThreshold || feelsLike <= FeelsLikePoorThreshold)
                        return "Very poor";
                    else
                        return "Fair";
                case "thunderstorm":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                    return "Very poor";
                case "snow":
                    if (windSpeed >= WindSpeedGoodThreshold)
                        return "Poor";
                    else
                        return "Good";
                case "fog":
                    if (temperature <= TemperaturePoorThreshold || feelsLike <= FeelsLikePoorThreshold || windSpeed >= WindSpeedVeryPoorThreshold)
                        return "Very poor";
                    else
                        return "Poor";
                default:
                    return "Uncertain";
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
                            return "Light jacket";
                        else if (humidity >= HumidityLowThreshold && humidity < HumidityModerateThreshold)
                            return "Long sleeves";
                        else
                            return "T-shirt";
                    }
                    else if (temperature <= TemperaturePoorThreshold || humidity >= HumidityHighThreshold)
                        return "Shorts";
                    else
                        return "Long sleeves";
                case "cloudy":
                case "partly cloudy":
                case "mostly cloudy":
                case "few clouds":
                case "overcast clouds":
                    if (temperature <= TemperatureFairThreshold || windSpeed >= WindSpeedFairThreshold || humidity >= HumidityHighThreshold || temperature <= TemperatureGoodThreshold || windSpeed >= WindSpeedGreatThreshold)
                        return "Long sleeves";
                    else
                        return "Light jacket";
                case "rain":
                case "light rain":
                case "moderate rain":
                case "drizzle":
                    if (windSpeed >= WindSpeedPoorThreshold)
                        return "Long sleeves";
                    else if (temperature < TemperaturePoorThreshold || humidity >= HumidityModerateThreshold)
                        return "Light jacket";
                    else
                        return "Long sleeves";
                case "thunderstorm":
                case "thunderstorm with rain":
                case "thunderstorm with heavy rain":
                    return "Heavy coat";
                case "snow":
                    if (windSpeed >= WindSpeedGoodThreshold)
                        return "Heavy coat";
                    else
                        return "Long sleeves";
                case "fog":
                    if (temperature < TemperaturePoorThreshold || humidity >= HumidityModerateThreshold || windSpeed >= WindSpeedVeryPoorThreshold)
                        return "Heavy coat";
                    else
                        return "Long sleeves";
                default:
                    return "Uncertain";
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
        public async Task MinMaxTemp(string location, DateTime Date)
        {
            try
            {                
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                var json = await httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                WeatherParameters.root output = result;                   
                var dayTemps = new List<double>();
                var nightTemps = new List<double>();

                foreach (var item in output.list)
                {
                    if (item.dt_txt.Date == Date.Date)
                    {
                        if (item.dt_txt.Hour >= 6 && item.dt_txt.Hour <= 18)
                        {
                            dayTemps.Add(item.main.temp_max);

                                DayHighTempTextBlock.Text = "The High will be " + Math.Round(item.main.temp_max) + "°C";
                                DayWeatherDescTextBlock.Text = MainWindow.GenerateWeatherMessage(item.weather[0].description);
                        }
                        else if (item.dt_txt.Hour >= 18 && item.dt_txt.Hour <= 24)
                        {
                            nightTemps.Add(item.main.temp_min);
                              
                                NightLowTempTextBlock.Text = "The Low will be " + Math.Round(item.main.temp_min) + "°C";
                                NightWeatherDescTextBlock.Text = MainWindow.GenerateWeatherMessage(item.weather[0].description);
                                
                        }
                    }
                }

                if (dayTemps.Any())
                {
                    double avgDayTemp = Math.Round(dayTemps.Average());
                    int avgDaytemp = Convert.ToInt32(avgDayTemp);
                    DayHighTempTextBlock.Text = "The High will be " + avgDaytemp.ToString("F2") + "°C";
                }
                else
                {
                    DayHighTempTextBlock.Text = "--";
                }

                if (nightTemps.Any())
                {
                    double avgNightTemp = Math.Round(nightTemps.Average());
                    int avgNighttemp = Convert.ToInt32(avgNightTemp);
                    NightLowTempTextBlock.Text = "The Low will be " + (avgNighttemp.ToString("F2")) + "°C";
                }
                else
                {
                    NightLowTempTextBlock.Text = "--";
                }
                if (dayTemps.Any() && nightTemps.Any())
                {
                    var weatherInsights = new WeatherInsights
                    {
                        WeatherInsightsTemperature1 = Convert.ToInt32(dayTemps.Max()),
                        WeatherInsightsTemperature2 = Convert.ToInt32(nightTemps.Min()),
                        AvgHighTemp = Convert.ToInt32(dayTemps.Average()),
                        AvgLowTemp = Convert.ToInt32(nightTemps.Average()),
                        WeatherInsightsWeatherDescription1 = DayWeatherDescTextBlock.Text,
                        WeatherInsightsWeatherDescription2 = NightWeatherDescTextBlock.Text
                    };

                    var weatherdata = LoadWeatherdata(filePath);
                    weatherdata.weatherInsights = weatherInsights;
                    SaveWeatherData(weatherdata, filePath);
                }


            }
            catch (Exception)
            {
                var moreDetails = LoadWeatherdata(filePath);
                if (moreDetails.weatherInsights != null)
                {
                    // Update UI elements with last saved data
                    Dispatcher.Invoke(() =>
                    {
                        AvgHigh.Text = moreDetails.weatherInsights.AvgHighTemp.ToString();
                        AvgLow.Text = moreDetails.weatherInsights.AvgLowTemp.ToString();
                        DayWeatherDescTextBlock.Text = moreDetails.weatherInsights.WeatherInsightsWeatherDescription1.ToString();
                        NightWeatherDescTextBlock.Text = moreDetails.weatherInsights.WeatherInsightsWeatherDescription2.ToString();
                        DayHighTempTextBlock.Text = "The High will be " + moreDetails.weatherInsights.WeatherInsightsTemperature1.ToString() + "°C";
                        NightLowTempTextBlock.Text = "The Low will be " + moreDetails.weatherInsights.WeatherInsightsTemperature2.ToString().ToString() + "°C";

                    });
                }
            }
        }


        public void DetachLoadedEvent()
        {
            Loaded -= UserControl_Loaded;
        }

        public async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await Weeklydata(MainWindow.locationname);
            await MinMaxTemp(MainWindow.locationname, Date);
            await sunsetsrises(MainWindow.locationname, Date);
            await Suggestionsforday(MainWindow.locationname,Date);
            await  GetMoonTimes(Date.ToString(), MainWindow.locationname);
        }
    }
}
