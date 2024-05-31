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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using static GMap.NET.MapProviders.StrucRoads.SnappedPoint;
using Newtonsoft.Json.Linq;
using System.Security.Policy;


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
        public static string locationname= "hyderabad";
       
        private GMapControl gMapControl;
       // Hourly hourly = new Hourly();

        public MainWindow()
        {

            InitializeComponent();
            // Initialize GMapControl
            gMapControl = new GMapControl
            {
                MapProvider = GMapProviders.GoogleMap,
                MinZoom = 1,
                MaxZoom = 18,
                Zoom = 10,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Add the GMapControl to the MapContainer Grid
            MapContainer.Children.Add(gMapControl);

            // Set initial position to a default location
            gMapControl.Position = new PointLatLng(0, 0);
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
          await  WeeklyWeather(locationname);
          await  Dailyweather(locationname);
            locationname = "Hyderabad";
            recentLocations = readRecentLocations();
            foreach (Location primary in recentLocations)
            {
                if (primary.IsPrimary == true)
                {
                    locationname = primary.LocationName;
                }
            }

            // RefreshRecentLocationsPanel();
            // Bind the ComboBox to the list of locations
            foreach (var item in recentLocations)
            {
                CityComboBox.Items.Add(item.LocationName);
            }

            LoadMap(locationname);
        }
        public List<Location> readRecentLocations()
        {
            try
            {
                string fileName = "RecentWeatherData.json";
                List<Location> locations = new List<Location>();

                // Check if the file exists and read existing data
                if (System.IO.File.Exists(fileName))
                {
                    string existingJson = System.IO.File.ReadAllText(fileName);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        // Deserialize existing data
                        try
                        {
                            locations = JsonConvert.DeserializeObject<List<Location>>(existingJson);
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine("JSON deserialization error: " + jsonEx.Message);
                            MessageBox.Show("An error occurred while reading the locations data. Please check the data file.");
                        }
                    }
                }

                return locations;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving recent locations: " + ex.Message);
                return null;
            }
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
                    bool isValidCity = await IsValidLocationAsync(location);

                    if (isValidCity)
                    {
                        // Use city name
                        url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                    }
                    else
                    {
                        // Assume location is in "latitude,longitude" format
                        var coordinates = await GetCoordinatesFromGeoNamesAsync(location);

                        if (coordinates.HasValue)
                        {
                            string latitude = coordinates.Value.Latitude.ToString();
                            string longitude = coordinates.Value.Longitude.ToString();
                            url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apikey}";
                        }
                        else
                        {
                            throw new Exception("Invalid location and unable to retrieve coordinates.");
                        }
                    }
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
                            CityComboBox.Text = locationname;
                            Time.Content = localTime.ToString("hh:mm tt");
                            Temperature.Content = Math.Round(output.main.temp).ToString() + "°C";
                            temperature =Convert.ToInt32(Math.Round(output.main.temp));
                            WeatherType.Content = GenerateWeatherMessage(output.weather[0].description);
                            Description.Content = output.weather[0].main;
                            AirQualityValue.Text = AirQualityIndex(location);
                            double windspeed = output.wind.speed * 3.6 ;
                            double windDirectionDegree = output.wind.deg;
                            CalculateWindDirection(windDirectionDegree);
                            WindValue.Text = Math.Round(windspeed).ToString() + " Km/h";
                            Imagedata(location);
                            double humidity = output.main.humidity;
                            HumidityValue.Text = Math.Round(humidity).ToString() + "%";
                            double visibility = output.visibility / 1000.0;
                            VisibilityValue.Text = Math.Round(visibility).ToString() + " Km";
                            PressureValue.Text = Math.Round(output.main.pressure).ToString();
                            DewPointValue.Text = $"{CalculateDewPoint(temperature, humidity)} mm";

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
        public async static Task<bool> IsValidLocationAsync(string location)
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apikey}";

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
        public async static Task<(double Latitude, double Longitude)?> GetCoordinatesFromGeoNamesAsync(string location)
        {
            try
            {
                string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";
                using (HttpClient client = new HttpClient())
                {
                    string json = await client.GetStringAsync(apiUrl);
                    var result = JsonConvert.DeserializeObject<geonamesResult>(json);

                    if (result?.geonames != null && result.geonames.Count > 0)
                    {
                        var data = result.geonames[0];
                        if (double.TryParse(data.lat, out double latitude) && double.TryParse(data.lng, out double longitude))
                        {
                            return (latitude, longitude);
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Error fetching coordinates from GeoNames: {ex.Message}");
                return null;
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
                    bool isValidCity = await IsValidLocationAsync(location);

                    if (isValidCity)
                    {
                        // Use city name
                        url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                    }
                    else
                    {
                        // Assume location is in "latitude,longitude" format
                        var coordinates = await GetCoordinatesFromGeoNamesAsync(location);

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

                        HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
                        int cardIndex = 1;

                        foreach (var item in output.list)
                        {
                            DateTime currentDate = item.dt_txt;

                            if (uniqueDates.Contains(currentDate.Date)/* || currentDate.Hour != 9*/)
                            {
                                continue;
                            }

                            uniqueDates.Add(currentDate.Date);

                            if (cardIndex > 7)
                            {
                                break;
                            }

                            // Update the weather card based on cardIndex
                            UpdateWeatherCard(cardIndex, currentDate, item);

                            cardIndex++;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"HTTP request failed with status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void UpdateWeatherCard(int cardIndex, DateTime date, WeatherParameters.ListItem item)
        {
            string dateName = $"WeatherDate{cardIndex}";
            string iconName = $"Icon{cardIndex}";
            string maxTempName = $"MaxDayTemperature{cardIndex}";
            string minTempName = $"MinDayTemperature{cardIndex}";
            string dewPointName = $"DewDayPoint{cardIndex}";
            string descriptionName = $"DayDescription{cardIndex}";

            var weatherDate = this.FindName(dateName) as TextBlock;
            var icon = this.FindName(iconName) as Image;
            var maxTemp = this.FindName(maxTempName) as TextBlock;
            var minTemp = this.FindName(minTempName) as TextBlock;
            var dewPoint = this.FindName(dewPointName) as TextBlock;
            var description = this.FindName(descriptionName) as TextBlock;

            if (weatherDate != null)
                weatherDate.Text = date.ToString("dd MMM");

            if (maxTemp != null)
                maxTemp.Text = $"{Convert.ToInt32(item.main.temp_max)}°C";

            if (minTemp != null)
                minTemp.Text = $"{Convert.ToInt32(item.main.temp_min)}°C";

            if (dewPoint != null)
                dewPoint.Text = $"{CalculateDewPoint(item.main.temp, item.main.humidity)} mm";

            if (description != null)
                description.Text = GenerateWeatherMessage(item.weather[0].description);

            if (icon != null)
            {
                icon.Source = await GetImage(item.weather[0].icon);
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
                        bool isValidCity = await IsValidLocationAsync(location);

                        if (isValidCity)
                        {
                            // Use city name
                            url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                        }
                        else
                        {
                            // Assume location is in "latitude,longitude" format
                            var coordinates = await GetCoordinatesFromGeoNamesAsync(location);

                            if (coordinates.HasValue)
                            {
                                string latitude = coordinates.Value.Latitude.ToString();
                                string longitude = coordinates.Value.Longitude.ToString();
                                url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apikey}";
                            }
                            else
                            {
                                throw new Exception("Invalid location and unable to retrieve coordinates.");
                            }
                        }
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                            WeatherParameters.root output = result;
                            string icon = output.weather[0].icon;
                            Iconlabel = icon;
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
        public void CalculateWindDirection(double degree)
        {
            try
            {
                degree %= 360;
                // Create a RotateTransform
                RotateTransform rotateTransform = new RotateTransform(degree);

                // Apply the RotateTransform to the TextBlock
                WindDirection.RenderTransform = rotateTransform;
                WindDirection.RenderTransformOrigin = new Point(0.5, 0.5); // Set rotation origin to center

            }
            catch (Exception)
            {
                
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
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(url).Result; // Synchronous call
                        if (response.IsSuccessStatusCode)
                        {
                            url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";

                        }
                        else
                        {
                            string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";
                            using (HttpClient client1 = new HttpClient())
                            {
                                string json1 = client1.GetStringAsync(apiUrl).Result; // Synchronous call
                                var result1 = JsonConvert.DeserializeObject<geonamesResult>(json1);

                                if (result1?.geonames != null && result1.geonames.Count > 0)
                                {
                                    var data = result1.geonames[0];
                                    if (double.TryParse(data.lat, out double latitude1) && double.TryParse(data.lng, out double longitude1))
                                    {
                                        url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude1}&lon={longitude1}&units=metric&appid={apikey}";

                                    }
                                }
                            }
                        }

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
        private async void LoadMap(string location)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    string apiKey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apiKey}";
                    bool isValidCity = await IsValidLocationAsync(location);

                    if (isValidCity)
                    {
                        // Use city name
                        url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apiKey}";
                    }
                    else
                    {
                        // Assume location is in "latitude,longitude" format
                        var coordinates = await GetCoordinatesFromGeoNamesAsync(location);

                        if (coordinates.HasValue)
                        {
                            string latitude = coordinates.Value.Latitude.ToString();
                            string longitude = coordinates.Value.Longitude.ToString();
                            url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apiKey}";
                        }
                        else
                        {
                            throw new Exception("Invalid location and unable to retrieve coordinates.");
                        }
                    }
                    var json = web.DownloadString(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;
                    // Update the position of the existing GMapControl
                    gMapControl.Position = new PointLatLng(output.coord.lat, output.coord.lon);
                    gMapControl.Markers.Clear();

                    // Create and add a heat map overlay
                // CreateHeatMapOverlay(output.coord.lat, output.coord.lon,gMapControl.Zoom);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading the map: {ex.Message}");
            }
        }
        private void CreateHeatMapOverlay(double lat, double lon, double zoom)
        {
            // Create a canvas for heat map overlay
            Canvas heatMapCanvas = new Canvas
            {
                Width = gMapControl.ActualWidth,
                Height = gMapControl.ActualHeight,
                Background = Brushes.Transparent
            };

            // Calculate the tile coordinates based on the provided latitude and longitude
            int zoomLevel = Convert.ToInt32(zoom); // Adjust the zoom level as needed
            zoomLevel = 1;
            int xTile = (int)(Math.Floor((lon + 180) / 360 * Math.Pow(2, zoomLevel)));
            int yTile = (int)(Math.Floor((1 - Math.Log(Math.Tan(lat * Math.PI / 180) + 1 / Math.Cos(lat * Math.PI / 180)) / Math.PI) / 2 * Math.Pow(2, zoomLevel)));

            // Replace this with your actual API key for OpenWeatherMap
            string apiKey = "329cec969cefc40090ac7b5d60221eaf";

            // Define the base URL for OpenWeatherMap tile images
            string baseUrl = $"https://tile.openweathermap.org/map/temp_new/{zoomLevel}/{xTile}/{yTile}.png?appid={apiKey}";

            // Create the image element for the tile
            Image tileImage = new Image
            {
                Width = gMapControl.ActualWidth, // Tile width
                Height = gMapControl.ActualHeight, // Tile height
                Opacity = 1
            };

            // Set the source of the tile image
            tileImage.Source = new BitmapImage(new Uri(baseUrl));

            // Position the tile image on the canvas
            Canvas.SetLeft(tileImage, 0);
            Canvas.SetTop(tileImage, 0);

            // Add the tile image to the heat map canvas
            heatMapCanvas.Children.Add(tileImage);

            // Add the heat map canvas to the GMapControl
            MapContainer.Children.Add(heatMapCanvas);
        }

        public void RecentLocations(string location, bool isPrimary)
        {
            try
            {
                string fileName = "RecentWeatherData.json";
                List<Location> locations = new List<Location>();

                // Check if the file exists and read existing data
                if (System.IO.File.Exists(fileName))
                {
                    string existingJson = System.IO.File.ReadAllText(fileName);
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        // Deserialize existing data
                        locations = JsonConvert.DeserializeObject<List<Location>>(existingJson);
                    }
                }
                // Add or update the primary location
                if (isPrimary)
                {
                    var primaryLocation = locations.FirstOrDefault(loc => loc.LocationName == location);
                    if (primaryLocation != null)
                    {
                        foreach (var loc in locations)
                        {
                            loc.IsPrimary = false;
                        }
                        primaryLocation.LocationName = location;
                        primaryLocation.IsPrimary = true;
                        // Move primaryLocation to the beginning of the list
                        locations.Remove(primaryLocation); // Remove from current position
                        locations.Insert(0, primaryLocation); // Insert at index 0

                    }
                    else if (!locations.Any(loc => loc.LocationName == location))
                    {
                        locations.Insert(0, new Location { LocationID = GenerateId(locations), LocationName = location, IsPrimary = true, temperature = temperature, iconlabel = Iconlabel });

                    }
                }
                else
                {
                    if (!locations.Any(loc => loc.LocationName == location))
                    {
                        locations.Add(new Location { LocationID = GenerateId(locations), LocationName = location, IsPrimary = false, temperature = temperature, iconlabel = Iconlabel });

                        var recentNonPrimaryLocations = locations.Where(loc => !loc.IsPrimary).OrderByDescending(loc => loc.LocationID).Take(5).ToList();

                        var primaryLocation = locations.FirstOrDefault(loc => loc.IsPrimary);
                        if (primaryLocation != null)
                        {
                            locations = new List<Location> { primaryLocation };
                            locations.AddRange(recentNonPrimaryLocations);
                        }
                        else
                        {
                            locations = recentNonPrimaryLocations;
                        }
                    }
                }
                // Serialize the updated list to JSON format
                string jsonData = JsonConvert.SerializeObject(locations, Formatting.Indented);

                // Write the JSON data to a file
                try
                {
                    System.IO.File.WriteAllText(fileName, jsonData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while writing to the file: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving recent locations: " + ex.Message);
            }
        }
        private int GenerateId(List<Location> locations) // for locationId autogenerate
        {
            return (locations.Count > 0) ? locations.Max(loc => loc.LocationID) + 1 : 1;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListBox.Items.Clear();
            ListBox.Visibility = Visibility.Visible;
            ListBoxData();
        }


        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBox.SelectedIndex>0)
            {             
                    var selectedItem = ListBox.SelectedItem.ToString();
                    SearchTextBox.Text = selectedItem;
                    locationname = selectedItem;
                    LocationName.Content = locationname;
                    LocationSearched();

            }
            ListBox.Visibility = Visibility.Collapsed;
        }
        private void UpdateComboBox()
        {
            try
            {
                // Clear existing items 
                //CityComboBox.DataSource = null;
                CityComboBox.Items.Clear();
                //Add recent locations to the ComboBox
                foreach (var location in recentLocations)
                {

                    CityComboBox.Items.Add(location.LocationName);
                    if (location.IsPrimary)
                    {
                        CityComboBox.Text = location.LocationName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured updatecombo");
                MessageBox.Show("A error while updating comboBox" + ex.Message);
            }
        }
        public async void LocationSearched()
        {
            await  Dailyweather(locationname);
            await  WeeklyWeather(locationname);
            //hourly.hourlydata(locationname, DateTime.Now.Date);
            LoadMap(locationname);
            var graph = SummaryGraph(locationname, DateTime.Today); 
            DataContent.Content = graph;
            RecentLocations(locationname, false);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Summary summary = new Summary();
            if (e.Source is TabControl tabControl1 && tabControl1.SelectedIndex == 0) // Check if the selected tab is the first tab (index 0)
            {
                
                // Call the SummaryGraph method here
                var graph = SummaryGraph(locationname, DateTime.Today); // Replace "YourLocation" with the actual location and DateTime.Today with the desired date
                DataContent.Content = graph;
            }
            else if (e.Source is TabControl tabControl2 && tabControl2.SelectedIndex == 2) // Check if tab 3 is selected
            {
                
                MoreDetails moreDetails = new MoreDetails();
                DataContent.Content = moreDetails;
            }
            else if (e.Source is TabControl tabControl3 && tabControl3.SelectedIndex == 1) // Check if tab 3 is selected
            {
                Hourly hourly = new Hourly();
                DataContent.Content = hourly;
            }
        }
        public UserControl SummaryGraph(string location, DateTime date)
        {
            using (WebClient web = new WebClient())
            {
                try
                {
                    string apikey = "329cec969cefc40090ac7b5d60221eaf";
                    string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = client.GetAsync(url).Result; // Synchronous call
                        if (response.IsSuccessStatusCode)
                        {
                            url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";

                        }
                        else
                        {
                            string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";
                            using (HttpClient client1 = new HttpClient())
                            {
                                string json1 = client1.GetStringAsync(apiUrl).Result; // Synchronous call
                                var result1 = JsonConvert.DeserializeObject<geonamesResult>(json1);

                                if (result1?.geonames != null && result1.geonames.Count > 0)
                                {
                                    var data = result1.geonames[0];
                                    if (double.TryParse(data.lat, out double latitude1) && double.TryParse(data.lng, out double longitude1))
                                    {
                                        url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude1}&lon={longitude1}&units=metric&appid={apikey}";

                                    }
                                }
                            }
                        }
                        var json = web.DownloadString(url);
                        var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                        WeatherParameters.root output = result;

                        var temperatures = new Dictionary<DateTime, int>();
                        foreach (var item in output.list)
                        {
                            if (item.dt_txt.Date == date.Date)
                            {
                                temperatures.Add(item.dt_txt, Convert.ToInt32(item.main.temp));
                            }
                        }

                        return GenerateHourlyLineChart(temperatures);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    return null;
                }
            }
        }

        private UserControl GenerateHourlyLineChart(Dictionary<DateTime, int> temperatures)
        {
            try
            {
                if (temperatures == null || temperatures.Count == 0)
                    throw new ArgumentException("Temperature list cannot be null or empty.");

                return  new Summary(temperatures);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            locationname = SearchTextBox.Text;
            LocationName.Content = locationname;
            LocationSearched();

        }
        //private bool IsSummaryTabActive = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Child is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children)
                {
                    if (child is TextBlock textBlock && textBlock.Name.StartsWith("WeatherDate"))
                    {
                        if (DateTime.TryParse(textBlock.Text, out DateTime selectedDate))
                        {                    // Assuming you have a method to get the location

                            if (TabControl.SelectedIndex == 0)
                            {
                                ShowSummaryGraph(locationname, selectedDate);

                            }
                            else if (TabControl.SelectedIndex == 1)
                            {
                                Hourly hourly = new Hourly();
                                hourly.hourlydata(locationname, selectedDate);
                                DataContent.Content = hourly;
                            }
                        }
                        break;
                    }
                }
            }
        }
        private void ShowSummaryGraph(string location, DateTime date)
        {
            var summaryGraph = SummaryGraph(location, date);
            if(summaryGraph != null)
            {
                DataContent.Content = summaryGraph;
            }
        }
        private string GetSelectedLocation()
        {
            return CityComboBox.SelectedItem != null ? CityComboBox.SelectedItem.ToString() : "DefaultLocation";
            
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
    
