using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
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
using Newtonsoft.Json;

namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Location> recentLocations = new List<Location>();
        public int temperature;
        public string Iconlabel;
        public static int Width;
        public static string locationname;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dailyweather("hyderabad");
        }
        public static List<string> SearchPlaces(string query)
        {
            try
            {
                string apiUrl = $"http://api.geonames.org/searchJSON?name_startsWith={query}&maxRows=10&username=avinash1547";
                using (WebClient webClient = new WebClient())
                {
                    string json = webClient.DownloadString(apiUrl);
                    RootObject data = JsonConvert.DeserializeObject<RootObject>(json);

                    // Filter out duplicate places based on name
                    HashSet<string> nameSet = new HashSet<string>();
                    List<string> uniquePlaces = new List<string>();
                    foreach (var place in data.geonames)
                    {
                        if (!nameSet.Contains(place.name))
                        {
                            nameSet.Add(place.name);
                            string displayName = FormatDisplayName(place);
                            uniquePlaces.Add(displayName);
                        }
                    }
                    return uniquePlaces;
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions here, for simplicity, return null
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
        private static string FormatDisplayName(geonames place)
        {
            try
            {
                List<string> nameParts = new List<string>();
                if (!string.IsNullOrEmpty(place.name))
                {
                    nameParts.Add(place.name);
                }
                if (!string.IsNullOrEmpty(place.adminName1))
                {
                    nameParts.Add(place.adminName1);
                }
                if (!string.IsNullOrEmpty(place.countryName))
                {
                    nameParts.Add(place.countryName);
                }
                if (!string.IsNullOrEmpty(place.postalCode))
                {
                    nameParts.Add(place.postalCode);
                }

                return string.Join(", ", nameParts);
            }
            catch (Exception)
            {
                return "No results found";
            }
        }
        private static string FormatDisplayNameForpostalcodes(PostalCodeData place)
        {
            try
            {
                List<string> nameParts = new List<string>();
                if (!string.IsNullOrEmpty(place.placeName))
                {
                    nameParts.Add(place.placeName);
                }
                if (!string.IsNullOrEmpty(place.adminName1))
                {
                    nameParts.Add(place.adminName1);
                }
                if (!string.IsNullOrEmpty(place.adminName2))
                {
                    nameParts.Add(place.adminName2);
                }
                if (!string.IsNullOrEmpty(place.adminName3))
                {
                    nameParts.Add(place.adminName3);
                }
                if (!string.IsNullOrEmpty(place.countryCode))
                {
                    nameParts.Add(place.countryCode);
                }
                if (!string.IsNullOrEmpty(place.postalCode))
                {
                    nameParts.Add(place.postalCode);
                }

                return string.Join(", ", nameParts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating display name: {ex.Message}");
                return "No results found";
            }
        }
        public static List<string> PostalCodesForIndia(string postalCode)
        {
            try
            {
                string apiUrl = $"http://api.geonames.org/postalCodeSearchJSON?postalcode_startsWith={postalCode}&country=IN&maxRows=10&username=avinash1547";
                using (HttpClient httpClient = new HttpClient())
                {
                    string json = httpClient.GetStringAsync(apiUrl).Result;
                    // Deserialize the JSON response into appropriate classes
                    var rootObject = JsonConvert.DeserializeObject<PostalCodeRootObject>(json);
                    if (rootObject != null)
                    {
                        List<string> postalCodeSet = new List<string>();
                        foreach (var place in rootObject.postalCodes)
                        {
                            string displayName = FormatDisplayNameForpostalcodes(place);
                            postalCodeSet.Add(displayName);

                        }
                        return postalCodeSet;
                    }
                    return null; // Handle cases where rootObject or rootObject.postalCodes is null
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here, for simplicity, return null
                Console.WriteLine($"An error occurred while fetching postal codes: {ex.Message}");
                return null;
            }
        }
        public void ListBoxData()
        {
            try
            {
                string query = SearchTextBox.Text.Trim(); // Trim to remove leading and trailing whitespaces
                if (string.IsNullOrEmpty(query))
                {
                    return;
                }
                List<string> displayNames = SearchPlaces(query);
                List<string> Postalcodes = PostalCodesForIndia(query);
                if ((displayNames != null && displayNames.Count > 0) || (Postalcodes != null && Postalcodes.Count > 0))
                {
                    foreach (var displayName in displayNames)
                    {
                        ListBox.Items.Add(displayName);
                    }

                    foreach (var postalCode in Postalcodes)
                    {
                        ListBox.Items.Add(postalCode);
                    }

                }
                else
                {
                    ListBox.Items.Clear();
                    ListBox.Items.Add("No results found");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No results found");
            }

        }
        public async Task Dailyweather(string location)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                        WeatherParameters.root output = result;
                        DateTime dateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(output.dt).UtcDateTime;
                        DateTime localTime = dateTimeUtc.AddSeconds(output.timezone);

                        // Update UI elements for current weather
                        Dispatcher.Invoke(() =>
                        {
                            CityComboBox.Text = location;
                            Time.Content = localTime.ToString("hh:mm tt");
                            Temperature.Content = Math.Round(output.main.temp).ToString() + "°C";
                            WeatherType.Content = GenerateWeatherMessage(output.weather[0].description);
                            Description.Content = output.weather[0].main;
                            AirQualityValue.Text = AirQualityIndex(location);
                            double windspeed = output.wind.speed * 3.6;
                            double windDirectionDegree = output.wind.deg;
                            string windDirection = WindCalculator.CalculateWindDirection(windDirectionDegree);
                            WindValue.Text = Math.Round(windspeed).ToString() + " " + windDirection;
                            Imagedata(location);
                    ;
                        });
                    }
                    else
                    {
                        MessageBox.Show("Error fetching weather data. Status code: " + response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading weather details: " + ex.Message, "Error");
            }
        }
       
        public async Task WeeklyWeather(string location)
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";

                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<WeatherParameters.root>(jsonResponse);
                        WeatherParameters.root output = result;

                        HashSet<DateTime> uniqueDates = new HashSet<DateTime>();

                        // Initialize variables for grid layout
                       
                        // Define the horizontal and vertical spacing between tabs
                        int horizontalSpacing = 10;
                        int verticalSpacing = 10;

                        // Initialize variables to keep track of the current position
                        int x = 0;
                        int y = 0;

                        foreach (var item in output.list)
                        {
                            DateTime currentDate = item.dt_txt;

                            // Check if the current date is already in the HashSet
                            if (uniqueDates.Contains(currentDate.Date) || currentDate.Hour != 9)
                            {
                                // If the current date is already in the HashSet or not at 9 AM, skip this iteration
                                continue;
                            }

                            // Add the current date to the HashSet to mark it as seen
                            uniqueDates.Add(currentDate.Date);

                            // Create a new instance of HorizontalTab for each unique date and pass the data
                            HorizontalTab horizontalTab = new HorizontalTab();
                            horizontalTab.TabInfo(currentDate, Convert.ToInt32(item.main.temp_min), Convert.ToInt32(item.main.temp_max), item.weather[0].description, GenerateWeatherMessage(item.weather[0].description), CalculateDewPoint(item.main.temp, item.main.humidity), GetImage(item.weather[0].icon));
                            HorizontalTabData.Children.Add(horizontalTab);

                            // Update the current position for the next tab
                            x++;
                        }
                    }
                    else
                    {
                        // Handle HTTP error
                        MessageBox.Show($"HTTP request failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        public async Task<ImageSource> GetImage(string icondata)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string icon = icondata;
                    string iconUrl = $"http://openweathermap.org/img/wn/{icon}.png";

                    // Download the icon image from the URL
                    byte[] imageData = await client.GetByteArrayAsync(iconUrl);

                    // Convert byte array to BitmapImage
                    BitmapImage bitmapImage = new BitmapImage();
                    using (MemoryStream memoryStream = new MemoryStream(imageData))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                    }

                    // Return the BitmapImage as ImageSource
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading weather icon: " + ex.Message);
                return null; // Return null in case of error
            }
        }

        public async Task Imagedata(String location)
        {
            try
            {

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string apikey = "329cec969cefc40090ac7b5d60221eaf";
                        string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";

                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                            WeatherParameters.root output = result;
                            string icon = output.weather[0].icon;
                            string iconUrl = $"http://openweathermap.org/img/wn/{icon}.png";
                            // Download the icon image from the URL
                            byte[] imageData = await client.GetByteArrayAsync(iconUrl);

                            // Convert byte array to BitmapImage
                            BitmapImage bitmapImage = new BitmapImage();
                            using (MemoryStream memoryStream = new MemoryStream(imageData))
                            {
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = memoryStream;
                                bitmapImage.EndInit();
                            }

                            // Assign the BitmapImage to the Image control
                            Icon.Source = bitmapImage;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading weather icon: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading weather icon: " + ex.Message);
            }
        }
        public string AirQualityIndex(string location)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                    var json = web.DownloadString(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;
                    string url2 = $"http://api.openweathermap.org/data/2.5/air_pollution/history?lat={output.coord.lat}&lon={output.coord.lon}&start=1606223802&end=1606482999&appid={apikey}";
                    json = web.DownloadString(url2);
                    result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    output = result;
                    string airQuality;
                    foreach (var item in output.list)
                    {

                        switch (item.main.aqi)
                        {
                            case 1:
                                airQuality = "Good";
                                break;
                            case 2:
                                airQuality = "Fair";
                                break;
                            case 3:
                                airQuality = "Moderate";
                                break;
                            case 4:
                                airQuality = "Poor";
                                break;
                            case 5:
                                airQuality = "Very Poor";
                                break;
                            default:
                                airQuality = "--"; // Handle unexpected values
                                break;
                        }
                        return airQuality;
                    }
                    return "Unknown";
                }
            }
            catch (Exception)
            {
                return "Unknown";

            }

        }
        public double CalculateDewPoint(double temperatureCelsius, double humidityPercentage)
        {
            double dewPoint = temperatureCelsius - ((100 - humidityPercentage) / 5);
            return dewPoint;
        }
        public static string GenerateWeatherMessage(string weatherDescription)
        {
            try
            {

                switch (weatherDescription.ToLower())
                {
                    //  case "clear":
                    case "clear sky":
                        return "The skies are clear today.";
                    case "few clouds":
                        return "There are a few clouds in the sky.";

                    case "scattered clouds":
                        return "Expect scattered clouds today.";

                    case "broken clouds":
                        return "The sky is partly cloudy today.";

                    case "overcast clouds":
                        return "The sky is overcast with clouds.";

                    case "light rain":
                        return "Expect light rain showers today.";

                    case "moderate rain":
                        return "Expect moderate rain showers today.";

                    case "heavy rain":
                        return "Be prepared for heavy rain today.";

                    case "drizzle":
                        return "There's drizzle in the forecast today.";

                    case "thunderstorm with light rain":
                        return "There's drizzle in the forecast today.";
                    case "rain":
                        return "Expect rain showers today.";

                    case "thunderstorm with rain":
                        return "Expect thunderstorms with light rain today.";

                    case "thunderstorm with heavy rain":
                        return "Expect thunderstorms with heavy rain today.";

                    case "thunderstorm":
                        return "Be cautious, there might be thunderstorms.";

                    case "snow":
                        return "Prepare for snowfall today.";

                    case "mist":
                        return "There's mist in the air today.";

                    case "fog":
                        return "Be cautious, visibility might be low due to fog.";
                    case "haze":
                        return "Be cautious, there might be haze in the air today.";

                    default:
                        return "Weather conditions are uncertain.";
                }
            }
            catch (Exception)
            {
                return "Weather conditions are uncertain.";

            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListBox.Items.Clear();
            ListBox.Visibility = Visibility.Visible;
            ListBoxData();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (ListBox.SelectedItem != null)
            {
                SearchTextBox.Text = ListBox.SelectedItem.ToString();
            }
            ListBox.Visibility = Visibility.Collapsed;
        }
    }
}
public class geonames
{
    public string adminCode1 { get; set; }
    public string lng { get; set; }
    public int geonameId { get; set; }
    public string postalCode { get; set; }
    public string toponymName { get; set; }
    public string countryId { get; set; }
    public string fcl { get; set; }
    public string countryCode { get; set; }
    public string name { get; set; }
    public string fclName { get; set; }
    public AdminCodes1 adminCodes1 { get; set; }
    public string countryName { get; set; }
    public string fcodeName { get; set; }
    public string adminName1 { get; set; }
    public string lat { get; set; }
    public string fcode { get; set; }
}
public class geonamesResult
{
    public int totalResultsCount { get; set; }
    public List<geonames> geonames { get; set; }
}


public class AdminCodes1
{
    public string ISO3166_2 { get; set; }
}

public class RootObject
{
    public int totalResultsCount { get; set; }
    public List<geonames> geonames { get; set; }
}
public class Location
{
    public int LocationID { get; set; }
    public string LocationName { get; set; }
    public bool IsPrimary { get; set; }
    public int temperature { get; set; }
    public string iconlabel { get; set; }
}


public class PostalCodeRootObject
{
    public List<PostalCodeData> postalCodes { get; set; }
}

public class PostalCodeData
{
    public string adminCode1 { get; set; }
    public string adminCode2 { get; set; }
    public string adminCode3 { get; set; }
    public string adminName1 { get; set; }
    public string adminName2 { get; set; }
    public string adminName3 { get; set; }
    public string countryCode { get; set; }
    public string ISO3166_2 { get; set; }
    public double lat { get; set; }
    public double lng { get; set; }
    public string placeName { get; set; }
    public string postalCode { get; set; }
}


public class WindCalculator
{
    public static string CalculateWindDirection(double degree)
    {
        try
        {
            // Unicode arrow character representing the arrow
            char arrow = '➤';

            // Normalize the degree to the range [0, 360)
            degree %= 360;

            // Define the Unicode code points for different arrow characters
            int rightArrowCodePoint = 0x27A4; // Code point for '➤'
            int upArrowCodePoint = 0x2191; // Code point for '↑'
            int downArrowCodePoint = 0x2193; // Code point for '↓'
            int leftArrowCodePoint = 0x2190; // Code point for '←'

            // Calculate the new arrow character based on the degree
            if (degree >= 315 || degree < 45)
            {
                arrow = (char)rightArrowCodePoint; // Right
            }
            else if (degree >= 45 && degree < 135)
            {
                arrow = (char)upArrowCodePoint; // Up
            }
            else if (degree >= 135 && degree < 225)
            {
                arrow = (char)leftArrowCodePoint; // Left
            }
            else if (degree >= 225 && degree < 315)
            {
                arrow = (char)downArrowCodePoint; // Down
            }

            // Convert the character to a string and return
            return arrow.ToString();
        }
        catch (Exception)
        {
            return "unknown";
        }
    }
} 
    
