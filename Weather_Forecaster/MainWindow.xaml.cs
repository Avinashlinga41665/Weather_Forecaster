using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Threading;
using System.Threading;
using static Weather_Forecaster.UpdatedLocation;
using System.Net.NetworkInformation;
using System.Windows.Media.Animation;


namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Location> recentLocations = new List<Location>(); //for to record a list of recent locations that has been searched  
        public int temperature; //to store main temperature values
        public string Iconlabel; //to store the value of image that is displayed on the screen
        public static string locationname = "hyderabad"; //default value
        private Button selectedButton; //for 
        private CancellationTokenSource cts = new CancellationTokenSource();//to make lostbox for quick response for each time text changed in search te
        private static HttpClient httpClient = new HttpClient(); //serves as a tool for making HTTP requests to web services.
        private const string apiKey = "329cec969cefc40090ac7b5d60221eaf";//api key id
        private const string filePath = "LastUpdated.json";//a file path to store the lastupdated values 
        public bool NetworkAvailable = true; //to store status of the network 
        private GMapControl gMapControl; // to initilaize gmaps 
        public static double width;// to store the window screen value
        public MainWindow()
        {

            InitializeComponent();
            this.WindowState = WindowState.Maximized; // to auto maximize the screen when opened
            ListBox.Visibility = Visibility.Collapsed; //the listbox should not be visible
            // Initialize GMapControl with these properties
            gMapControl = new GMapControl
            {
                MapProvider = GMapProviders.GoogleMap,
                MinZoom = 1,
                MaxZoom = 18,
                Zoom = 10,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            // make the gMapControl visbile
            gMapControl.Visibility = Visibility.Visible;
            // Add the GMapControl to the MapContainer Grid
            MapContainer.Children.Add(gMapControl);
            PrimaryIcon.Visibility = Visibility.Hidden;
            // Set initial position to a default location
            gMapControl.Position = new PointLatLng(0, 0);
                // Subscribe to network availability changes
                NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChangedCallback);
           

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CityComboBox.Items.Clear(); //clear cityComboBox values
            recentLocations = readRecentLocations(); //to get all the values of recentlocation from the file
            foreach (Location primary in recentLocations)// search for primary location in it if it is present then store in locationname
            {
                if (primary.IsPrimary == true)
                {
                    locationname = primary.LocationName;
                }
            }
            foreach (Location primary in recentLocations)// add these recent location to the cityComboBox
            {
                CityComboBox.Items.Add(primary.LocationName);
            }
            CityComboBox.Text = locationname; // show the primary location in citycombobox when window is loaded
            await Task.WhenAll                // to run all methods concurrently to reduce the time taken to load values 
                (
           Dailyweather(locationname),
           WeeklyWeather(locationname),
           LoadMap(locationname)
               );
            PrimaryIcon.Visibility = Visibility.Visible; //to make primary icon visible
            PrimaryButton.Visibility = Visibility.Hidden;// and primarybutton invisible 
            LocationName.Content = locationname; //show the location name in LocationName label
            width = this.ActualWidth; // store the maximized width of screen
            ShowSummaryGraph(locationname, DateTime.Today);//show the summary graph below of today date
            RecentContainer();//to update recent container and show the recent locations

        }
        private async void NetworkAvailabilityChangedCallback(object sender, NetworkAvailabilityEventArgs e) //Avinash
        {
            
            if (!e.IsAvailable) // if the network is lost while using the application then show this message
            {
                 
                await Application.Current.Dispatcher.InvokeAsync( async () =>
                {
                    await DisplayErrorhandlingMessage("Network connection lost.", Colors.Red, Colors.Orange, false);
                    SearchButton.IsEnabled = false;
                    Refresh.IsEnabled = false;
                    NetworkAvailable = false;
                });
            }
            else // if the network is connected again while using the application then show this message
            {
                
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {                    
                    await DisplayErrorhandlingMessage("Network connection restored. You can refresh the data.", Colors.DarkGreen, Colors.LimeGreen, true);
                    SearchButton.IsEnabled = true;                   
                    Refresh.IsEnabled = true;
                    NetworkAvailable = true;
                });
            }
        }
        private async Task DisplayErrorhandlingMessage(string message, Color startColor, Color endColor, bool enableFadeOut) //Avinash 
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>              //setting properties of the display message
            {
                try
                {
                    // Create a gradient background using the provided colors
                    var gradientBrush = new LinearGradientBrush();
                    gradientBrush.StartPoint = new Point(0, 0);
                    gradientBrush.EndPoint = new Point(1, 0);
                    gradientBrush.GradientStops.Add(new GradientStop(startColor, 0.0));
                    gradientBrush.GradientStops.Add(new GradientStop(endColor, 1.0));

                    // Update message properties for better readability
                    Message.Text = message;
                    Message.Visibility = Visibility.Visible;
                    Message.Background = gradientBrush; // Set gradient background
                    Message.FontSize = 14; // Set font size
                    Message.FontFamily = new FontFamily("Segoe UI"); // Set font family
                    Message.FontStyle = FontStyles.Normal;
                    Message.Foreground = Brushes.White; // Set text color
                    Message.Padding = new Thickness(5); // Add padding for better spacing
                    Message.Margin = new Thickness(5); // Add margin for better separation from other elements
                   
                    if (enableFadeOut)
                    {
                        var fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
                        fadeInStoryboard.Begin(Message);

                        // Set a timer to start the fade-out animation after 3 seconds
                        var timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(8)
                        };
                        timer.Tick += (s, args) =>
                        {
                            // Begin fade-out animation
                            var fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");
                            fadeOutStoryboard.Completed += (snd, evt) =>
                            {
                                Message.Visibility = Visibility.Collapsed;
                            };
                            fadeOutStoryboard.Begin(Message);
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    else
                    {
                        var fadeInStoryboard = (Storyboard)FindResource("FadeInStoryboard");
                        fadeInStoryboard.Begin(Message);

                        // Set a timer to start the fade-out animation after 3 seconds
                        var timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(90000)
                        };
                        timer.Tick += (s, args) =>
                        {
                            // Begin fade-out animation
                            var fadeOutStoryboard = (Storyboard)FindResource("FadeOutStoryboard");
                            fadeOutStoryboard.Completed += (snd, evt) =>
                            {
                                Message.Visibility = Visibility.Collapsed;
                            };
                            fadeOutStoryboard.Begin(Message);
                            timer.Stop();
                        };
                        timer.Start();
                    }
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while displaying message: " + ex.Message);
                }
            });
        }

        public List<Location> readRecentLocations()
        {
            try
            {
                string fileName = "RecentWeatherData.json";   //file name for to store recent location data in json format
                List<Location> locations = new List<Location>();// a list of location that has stored in file

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

                // Create a list to store the result
                List<Location> result = new List<Location>();

                // Find and add the primary location first
                var primaryLocation = locations.FirstOrDefault(loc => loc.IsPrimary);
                if (primaryLocation != null)
                {
                    result.Add(primaryLocation);
                }

                // Add the most recent non-primary locations sorted in descending order of LocationID (or other identifier)
                var recentNonPrimaryLocations = locations
                    .Where(loc => !loc.IsPrimary)
                    .OrderByDescending(loc => loc.LocationID) // Assuming LocationID is used for ordering
                    .ToList();

                result.AddRange(recentNonPrimaryLocations);

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while retrieving recent locations: " + ex.Message);
                return null;
            }
        }

        public async static Task<List<string>> SearchPlaces(string query) //Avinash
        {
            try
            {    //to show the places that has been searched in the texbox to the listox
                string apiUrl = $"http://api.geonames.org/searchJSON?name_startsWith={query}&maxRows=10&username=avinash1547";

                string json = await httpClient.GetStringAsync(apiUrl);
                RootObject data = JsonConvert.DeserializeObject<RootObject>(json);

                // Filter out duplicate places based on name
                HashSet<string> nameSet = new HashSet<string>();
                List<string> uniquePlaces = new List<string>();
                foreach (var place in data.geonames) //names of the location from api
                {
                    if (!nameSet.Contains(place.name))//adding only unique names to the list
                    {
                        nameSet.Add(place.name);
                        string displayName = FormatDisplayName(place);
                        uniquePlaces.Add(displayName);
                    }
                }
                //return a list of place matching the query parameter
                return uniquePlaces;


            }
            catch (Exception ex)
            {
                // Handle exceptions here, for simplicity, return null
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
        private static string FormatDisplayName(geonames place) //Avinash
        {
            try
            {
                //format of the locationname that should be displayed in the listbox
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
        private static string FormatDisplayNameForpostalcodes(PostalCodeData place) //Avinash
        {
            try
            {
                //format of the locationname that should be displayed in the listbox
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
        public async static Task<List<string>> PostalCodesForIndia(string postalCode) //Avinash
        {
            try
            {
                string apiUrl = $"http://api.geonames.org/postalCodeSearchJSON?postalcode_startsWith={postalCode}&country=IN&maxRows=10&username=avinash1547";

                string json = await httpClient.GetStringAsync(apiUrl);
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
            catch (Exception ex)
            {
                // Handle exceptions here, for simplicity, return null
                Console.WriteLine($"An error occurred while fetching postal codes: {ex.Message}");
                return null;
            }
        }
        public async void ListBoxData() //Avinash
        {
            try
            {
                string query = SearchTextBox.Text.Trim();
                if (string.IsNullOrEmpty(query))
                {
                    return;
                }

                cts.Cancel();
                cts = new CancellationTokenSource();
                var token = cts.Token;

                await Task.Delay(300, token); // Debounce delay

                if (!token.IsCancellationRequested)
                {
                    var placeTask = SearchPlaces(query);
                    var postalCodeTask = PostalCodesForIndia(query);

                    await Task.WhenAll(placeTask, postalCodeTask);

                    var displayNames = placeTask.Result;
                    var postalCodes = postalCodeTask.Result;

                    ListBox.Items.Clear();

                    if ((displayNames != null && displayNames.Count > 0) || (postalCodes != null && postalCodes.Count > 0))
                    {
                        if (displayNames != null)
                        {
                            foreach (var name in displayNames)
                            {
                                ListBox.Items.Add(name);
                            }
                        }

                        if (postalCodes != null)
                        {
                            foreach (var code in postalCodes)
                            {
                                ListBox.Items.Add(code);
                            }
                        }
                    }
                    else
                    {
                        ListBox.Items.Add("No results found");
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) //Avinash
        {
            // Check if the mouse event target is not the ListBox
            if (!ListBox.IsMouseOver)
            {
                // Hide the ListBox
                ListBox.Visibility = Visibility.Collapsed;
            }
        }
        public async Task Dailyweather(string location)   //Avinash
        {
            try
            {

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
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                    WeatherParameters.root output = result;
                    DateTime dateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(output.dt).UtcDateTime;
                    DateTime localTime = dateTimeUtc.AddSeconds(output.timezone);

                    // Update UI elements for current weather
                    await Imagedata(location);
                    Dispatcher.Invoke(() =>
                    {
                        CityComboBox.Text = locationname;
                        LocationName.Content = locationname;
                        CityComboBox.SelectedItem = locationname;
                        Time.Content = localTime.ToString("hh:mm tt");
                        Temperature.Content = Math.Round(output.main.temp).ToString() + "°C";
                        temperature = Convert.ToInt32(Math.Round(output.main.temp));
                        WeatherType.Content = GenerateWeatherMessage(output.weather[0].description);
                        Description.Content = output.weather[0].main;
                        AirQualityValue.Text = AirQualityIndex(location);
                        double windspeed = output.wind.speed * 3.6;
                        double windDirectionDegree = output.wind.deg;
                        CalculateWindDirection(windDirectionDegree);
                        WindValue.Text = Math.Round(windspeed).ToString() + " Km/h";
                        double humidity = output.main.humidity;
                        HumidityValue.Text = Math.Round(humidity).ToString() + "%";
                        double visibility = output.visibility / 1000.0;
                        VisibilityValue.Text = Math.Round(visibility).ToString() + " Km";
                        PressureValue.Text = Math.Round(output.main.pressure).ToString();
                        DewPointValue.Text = $"{CalculateDewPoint(temperature, humidity)} mm";

                    });

                    var lastUpdated = new LastUpdated
                    {
                        Location = location,
                        Time = localTime,
                        Temperature = Convert.ToInt32(Math.Round(output.main.temp)),
                        WeatherType = output.weather[0].main,
                        WeatherDescription = output.weather[0].description,
                        AirQuality = AirQualityIndex(location),
                        Wind = (int)Math.Round(output.wind.speed * 3.6),
                        Humidity = (int)Math.Round(output.main.humidity),
                        Visibility = (int)Math.Round(output.visibility / 1000.0),
                        Pressure = Math.Round(output.main.pressure),
                        DewPoint = CalculateDewPoint(output.main.temp, output.main.humidity),
                    };
                    var weatherdata = LoadWeatherdata(filePath);
                    weatherdata.DailyWeather = lastUpdated;
                    SaveWeatherData(weatherdata, filePath);
                }

                else
                {
                    MessageBox.Show("Error fetching weather data. Status code: " + response.StatusCode);
                }

            }
            catch (Exception)
            {
                var lastUpdated = LoadWeatherdata(filePath);
                if (lastUpdated != null)
                {
                    NetworkAvailable = false;
                    // Update UI elements with last saved data
                    Dispatcher.Invoke(() =>
                    {
                        CityComboBox.Text = lastUpdated.DailyWeather.Location;
                        locationname = lastUpdated.DailyWeather.Location;   
                        Time.Content = lastUpdated.DailyWeather.Time.ToString("hh:mm tt");
                        Temperature.Content = lastUpdated.DailyWeather.Temperature.ToString() + "°C";
                        WeatherType.Content = lastUpdated.DailyWeather.WeatherType;
                        Description.Content = lastUpdated.DailyWeather.WeatherDescription;
                        AirQualityValue.Text = lastUpdated.DailyWeather.AirQuality.ToString();
                        WindValue.Text = lastUpdated.DailyWeather.Wind.ToString() + " Km/h";
                        HumidityValue.Text = lastUpdated.DailyWeather.Humidity.ToString() + "%";
                        VisibilityValue.Text = lastUpdated.DailyWeather.Visibility.ToString() + " Km";
                        PressureValue.Text = lastUpdated.DailyWeather.Pressure.ToString();
                        DewPointValue.Text = lastUpdated.DailyWeather.DewPoint.ToString() + " mm";
                    });
                }
                else
                {
                    MessageBox.Show("Error: No internet connection and no saved data available.");
                }
            }
        } 
        public async static Task<bool> IsValidLocationAsync(string location)   //Avinash
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={apikey}";

                
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    return response.IsSuccessStatusCode;
                
            }
            catch
            {
                return false;
            }
        }
        public async static Task<(double Latitude, double Longitude)?> GetCoordinatesFromGeoNamesAsync(string location) //Avinash
        {
            try
            {
                string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";
               
                    string json = await httpClient.GetStringAsync(apiUrl);
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
            catch (Exception ex)
            {
                // Log the error (you can replace this with your logging mechanism)
                Console.WriteLine($"Error fetching coordinates from GeoNames: {ex.Message}");
                return null;
            }
        }

        public async Task WeeklyWeather(string location) //Avinash
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apikey}";

               
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
                        var weatherData = UpdatedLocation.LoadWeatherdata(filePath);
                        HashSet<DateTime> uniqueDates = new HashSet<DateTime>();
                        int cardIndex = 1;
                         weatherData.WeeklyWeather.Clear();
                        foreach (var item in output.list)
                        {
                            DateTime currentDate = item.dt_txt;

                            if (uniqueDates.Contains(currentDate.Date)/* || currentDate.Hour != 9*/)
                            {
                                continue;
                            }

                            uniqueDates.Add(currentDate.Date);


                         var weeklyweatherlist = new UpdatedLocation.WeeklyWeather
                         {
                            MinTemperature = item.main.temp_min,
                            MaxTemperature = item.main.temp_max,
                            Description = GenerateWeatherMessage(item.weather[0].description),
                            WeeklyDewpoint = CalculateDewPoint(item.main.temp, item.main.humidity)
                            
                         };
                        if(weatherData.WeeklyWeather.Count<7)
                        {
                            weatherData.WeeklyWeather.Add(weeklyweatherlist);
                        }
                        if (cardIndex > 7)
                        {
                            break;
                        }
                        UpdateWeatherCard(cardIndex, currentDate, item);
                        // Update the weather card based on cardIndex
                        cardIndex++;
                        }
                    UpdatedLocation.SaveWeatherData(weatherData, filePath);
                    }
                    else
                    {
                        MessageBox.Show($"HTTP request failed with status code: {response.StatusCode}");
                    }
                
            }
            catch (Exception)
            {
                var weeklyWeather = UpdatedLocation.LoadWeatherdata(filePath);
                for (int i = 0; i < weeklyWeather.WeeklyWeather.Count; i++)
                {
                    if (i >= 7) break;

                    var date = DateTime.Today.AddDays(i - 1);
                    UpdateWeatherCardFromLocalData(i, date, weeklyWeather.WeeklyWeather[i]);
                }
            }
            
        }
        private void UpdateWeatherCardFromLocalData(int cardIndex, DateTime date, UpdatedLocation.WeeklyWeather weatherData) //Avinash
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
            if (date.Date == DateTime.Today.Date)
            {
                weatherDate.Text = "Today";
            }
            if (date.Date == DateTime.Today.AddDays(1).Date)
            {
                weatherDate.Text = "Tomorrow";
            }
            //if (date.Date == DateTime.Today.AddDays(-1).Date)
            //{
            //    weatherDate.Text = "Yesterday";
            //}

            if (maxTemp != null)
                maxTemp.Text = $"{Convert.ToInt32(weatherData.MaxTemperature)}°C";

            if (minTemp != null)
                minTemp.Text = $"{Convert.ToInt32(weatherData.MinTemperature)}°C";

            if (dewPoint != null)
                dewPoint.Text = $"{weatherData.WeeklyDewpoint} mm";

            if (description != null)
                description.Text = GenerateWeatherMessage("No description available");
        }
        private async void UpdateWeatherCard(int cardIndex, DateTime date, WeatherParameters.ListItem item) //Avinash
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
                if (date.Date == DateTime.Today.Date)
                {
                    weatherDate.Text = "Today";
                }
                if (date.Date == DateTime.Today.AddDays(1).Date)
                {
                weatherDate.Text = "Tomorrow";
                }
                if (date.Date == DateTime.Today.AddDays(-1).Date)
                {
                weatherDate.Text = "Yesterday";
                }

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

        public async static Task<ImageSource> GetImage(string icondata) //Avinash
        {
            try
            {
               
                    string icon = icondata;
                    string iconUrl = $"http://openweathermap.org/img/wn/{icon}.png";

                    // Download the icon image from the URL
                    byte[] imageData = await httpClient.GetByteArrayAsync(iconUrl);

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
            catch (Exception ex)
            {
                MessageBox.Show("Error loading weather icon: " + ex.Message);
                return null; // Return null in case of error
            }
        }


        public async Task Imagedata(String location) //Avinash
        {
            try
            {

                try
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
                        HttpResponseMessage response = await httpClient.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            var result =  JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                            WeatherParameters.root output = result;
                            string icon = output.weather[0].icon;
                            Iconlabel = icon;
                            string iconUrl = $"http://openweathermap.org/img/wn/{icon}.png";
                            // Download the icon image from the URL
                            byte[] imageData = await httpClient.GetByteArrayAsync(iconUrl);

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
        public void CalculateWindDirection(double degree)  //Avinash
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
        public string AirQualityIndex(string location) //Avinash
        {
            try
            {
                string apikey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";
                  
                        HttpResponseMessage response = httpClient.GetAsync(url).Result; // Synchronous call
                if (response.IsSuccessStatusCode)
                {
                    url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apikey}";

                }
                else
                {
                    string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";

                    string json1 = httpClient.GetStringAsync(apiUrl).Result; // Synchronous call
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
                var json = httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json.Result);
                WeatherParameters.root output = result;
                string url2 = $"http://api.openweathermap.org/data/2.5/air_pollution/history?lat={output.coord.lat}&lon={output.coord.lon}&start=1606223802&end=1606482999&appid={apikey}";
                var json2 = httpClient.GetStringAsync(url2);
                result = JsonConvert.DeserializeObject<WeatherParameters.root>(json2.Result);
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
            catch (Exception)
            {
                return "Unknown";

            }

        }
        public double CalculateDewPoint(double temperatureCelsius, double humidityPercentage) //Avinash
        {
            double dewPoint = temperatureCelsius - ((100 - humidityPercentage) / 5);
            return dewPoint;
        }

        public static string GenerateWeatherMessage(string weatherDescription) //Avinash
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
        private async Task LoadMap(string location) //Avinash
        {
            try
            {
                gMapControl.Visibility = Visibility.Visible;
                string apiKey = "329cec969cefc40090ac7b5d60221eaf";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units=metric&appid={apiKey}";
                bool isValidCity = await IsValidLocationAsync(location);

                if (!isValidCity)
                {
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

                var json = await httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json);
                // Update the position of the existing GMapControl
                gMapControl.Position = new PointLatLng(result.coord.lat, result.coord.lon);
                gMapControl.Markers.Clear();

                // Create and add a heat map overlay
                // CreateHeatMapOverlay(result.coord.lat, result.coord.lon, gMapControl.Zoom);
            }
            catch (Exception)
            {
                // Clear the map and display an error message
                gMapControl.Position = new PointLatLng(0, 0); // Reset to a default location, e.g., (0, 0)
                gMapControl.Markers.Clear();
                gMapControl.Visibility = Visibility.Hidden;
            }
        }


        private void CreateHeatMapOverlay(double lat, double lon, double zoom) //Avinash
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

        public async Task RecentLocations(string location, bool isPrimary) //Avinash
        {
            try
            {
                string fileName = "RecentWeatherData.json";
                List<Location> locations = new List<Location>();

                // Check if the file exists and read existing data asynchronously
                if (System.IO.File.Exists(fileName))
                {
                    string existingJson;
                    using (var streamReader = new StreamReader(fileName))
                    {
                        existingJson = await streamReader.ReadToEndAsync();
                    }
                    if (!string.IsNullOrWhiteSpace(existingJson))
                    {
                        // Deserialize existing data
                        locations = JsonConvert.DeserializeObject<List<Location>>(existingJson);
                    }
                }

                // Handle primary location setting
                if (isPrimary)
                {
                    // Clear any existing primary location
                    foreach (var loc in locations)
                    {
                        loc.IsPrimary = false;
                    }

                    // Check if the new location already exists in the list
                    var existingLocation = locations.FirstOrDefault(loc => loc.LocationName == location);
                    if (existingLocation != null)
                    {
                        existingLocation.IsPrimary = true;
                        // Move the primary location to the top of the list
                        locations.Remove(existingLocation);
                        locations.Insert(0, existingLocation);
                    }
                    else
                    {
                        // Add the new primary location if it doesn't exist
                        locations.Insert(0, new Location
                        {
                            LocationID = GenerateId(locations),
                            LocationName = location,
                            IsPrimary = true,
                            temperature = temperature,
                            iconlabel = Iconlabel
                        });
                    }
                }
                else
                {
                    // Add or update the non-primary location
                    var existingLocation = locations.FirstOrDefault(loc => loc.LocationName == location);
                    if (existingLocation == null)
                    {
                        locations.Add(new Location
                        {
                            LocationID = GenerateId(locations),
                            LocationName = location,
                            IsPrimary = false,
                            temperature = temperature,
                            iconlabel = Iconlabel
                        });
                    }
                }

                // Ensure only the last 5 non-primary locations are stored
                var recentNonPrimaryLocations = locations
                    .Where(loc => !loc.IsPrimary)
                    .ToList();

                if (recentNonPrimaryLocations.Count > 5)
                {
                    recentNonPrimaryLocations = recentNonPrimaryLocations.Skip(recentNonPrimaryLocations.Count - 5).ToList();
                }

                // Preserve primary location at the top
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

                // Serialize the updated list to JSON format
                string jsonData = JsonConvert.SerializeObject(locations, Formatting.Indented);

                // Write the JSON data to a file asynchronously
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        await streamWriter.WriteAsync(jsonData);
                    }
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) //Avinash
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
            {
                SearchTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                SearchTextBoxPlaceholder.Visibility = Visibility.Hidden;
            }
            ListBox.Items.Clear();
            ListBox.Visibility = Visibility.Visible;
            ListBoxData();
        }


        private async void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Avinash
        {
            if (ListBox.SelectedIndex > 0) // Ensure there is a valid selection
            {
                var selectedItem = ListBox.SelectedItem as string; // Safely cast the selected item
                if (!string.IsNullOrEmpty(selectedItem)) // Check if the selected item is not null or empty
                {
                    SearchTextBox.Text = selectedItem;
                     locationname = selectedItem;
                     CityComboBox.SelectedItem = locationname;
                    await LocationSearched();
                    UpdateComboBox();
                    CityComboBox.Text = selectedItem;
                    recentLocations = readRecentLocations();
                    foreach (Location primary in recentLocations)
                    {
                        if (primary.IsPrimary == true && primary.LocationName==selectedItem)
                        {
                            PrimaryButton.Visibility = Visibility.Hidden;
                            PrimaryIcon.Visibility = Visibility.Visible;
                        }
                        if (primary.IsPrimary == false && primary.LocationName == selectedItem)
                        {
                            PrimaryButton.Visibility = Visibility.Visible;
                            PrimaryIcon.Visibility = Visibility.Hidden;
                        }
                    }

                }
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
                List<Location> recentLocations = readRecentLocations();
                //Add recent locations to the ComboBox
                foreach (var location in recentLocations)
                {

                    CityComboBox.Items.Add(location.LocationName);
                    CityComboBox.Text = locationname;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("A error while updating comboBox" + ex.Message);
            }
        }
        public async Task LocationSearched() //Avinash
        {
            await Dailyweather(locationname);
            await LoadMap(locationname);
            await WeeklyWeather(locationname);
            selectedButton = SummaryButton;
            ShowSummaryGraph(locationname, DateTime.Today);
            await RecentLocations(locationname, false);
        }
        public UserControl SummaryGraph(string location, DateTime date) //Avinash
        {
           
            try
            {
                string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apiKey}";
                   
                    HttpResponseMessage response = httpClient.GetAsync(url).Result; // Synchronous call
                    if (response.IsSuccessStatusCode)
                    {
                        url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&units=metric&appid={apiKey}";

                    }
                    else
                    {
                        string apiUrl = $"http://api.geonames.org/searchJSON?q={location}&maxRows=1&username=avinash1547";
                           
                            string json1 = httpClient.GetStringAsync(apiUrl).Result; // Synchronous call
                            var result1 = JsonConvert.DeserializeObject<geonamesResult>(json1);

                            if (result1?.geonames != null && result1.geonames.Count > 0)
                            {
                                var data = result1.geonames[0];
                                if (double.TryParse(data.lat, out double latitude1) && double.TryParse(data.lng, out double longitude1))
                                {
                                    url = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude1}&lon={longitude1}&units=metric&appid={apiKey}";

                                }
                            }
                        }
                        
                    var json2 = httpClient.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<WeatherParameters.root>(json2.Result);
                    WeatherParameters.root output = result;
                   var weatherData = UpdatedLocation.LoadWeatherdata(filePath);
                   var temperatures = new Dictionary<DateTime, int>();
                    var rainPercentages = new Dictionary<DateTime, int>();
                    weatherData.SummaryData.Clear();
                    foreach (var item in output.list)
                    {
                    if (item.dt_txt.Date == date.Date)
                    {
                        temperatures.Add(item.dt_txt, Convert.ToInt32(item.main.temp));
                        rainPercentages.Add(item.dt_txt, item.clouds.all);
                    }
                        var Temperaturelist = new UpdatedLocation.Summary
                             {
                             SummarDate = item.dt_txt,
                             SummaryTemperature = Convert.ToInt32(item.main.temp)

                             };
                        var rainpercentageslist = new UpdatedLocation.Summary
                        {
                            SummarDate = item.dt_txt,
                            SummaryRainPercentage = Convert.ToInt32(item.clouds.all)
                        };
                             if (weatherData.SummaryData != null)
                             {
                                weatherData.SummaryData.Add(Temperaturelist);
                                weatherData.SummaryData.Add(rainpercentageslist);
                             }
                        
                    
                    }

                SaveWeatherData(weatherData, filePath);
                return GenerateHourlyLineChart(temperatures, rainPercentages);
                    
            }
            catch (Exception)
            {
                var Summarygraph = LoadWeatherdata(filePath);
                var temperatures = new Dictionary<DateTime, int>();
                var rainPercentage = new Dictionary<DateTime, int>();
                temperatures.Clear();
                foreach (var item in Summarygraph.SummaryData)
                {
                    if (item.SummarDate.Date == date.Date.Date)
                    {
                        if (!temperatures.ContainsKey(item.SummarDate))
                        {
                            temperatures.Add(item.SummarDate, Convert.ToInt32(item.SummaryTemperature));
                            rainPercentage.Add(item.SummarDate, item.SummaryRainPercentage);
                        }
                    }
                }
                return GenerateHourlyLineChart(temperatures,rainPercentage);
            }
            
        }

        private UserControl GenerateHourlyLineChart(Dictionary<DateTime, int> temperatures,Dictionary<DateTime,int>RainPercentage) //Avinash
        {
            try
            {
                if (temperatures == null || temperatures.Count == 0)
                    throw new ArgumentException("Temperature list cannot be null or empty.");

                return new Summary(temperatures,RainPercentage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e) //Avinash
        {
            try
            {
                bool valid = await TextValidation(SearchTextBox.Text);
                if (!valid)
                {
                    return;
                }
                if (SearchTextBox.Text != null)
                {
                    locationname = SearchTextBox.Text;
                   // CityComboBox.Text = locationname;
                    CityComboBox.SelectedItem = locationname;
                    LocationName.Content = locationname;
                    await LocationSearched();
                    RecentContainer();
                    UpdateComboBox();
                    ListBox.Visibility = Visibility.Hidden;
                    recentLocations = readRecentLocations();
                    foreach (Location primary in recentLocations)
                    {
                        if (primary.IsPrimary == true && primary.LocationName == locationname)
                        {
                            PrimaryButton.Visibility = Visibility.Hidden;
                            PrimaryIcon.Visibility = Visibility.Visible;
                        }
                        if (primary.IsPrimary == false && primary.LocationName == locationname)
                        {
                            PrimaryButton.Visibility = Visibility.Visible;
                            PrimaryIcon.Visibility = Visibility.Hidden;
                        }
                    }
                   // CityComboBox.Text = SearchTextBox.Text;
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Searchbutton error!");
            }

        }
        private  void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(NetworkAvailable == true)
            {
                if (sender is Border border && border.Child is StackPanel stackPanel)
                {
                    foreach (var child in stackPanel.Children)
                    {
                        if (child is TextBlock textBlock && textBlock.Name.StartsWith("WeatherDate"))
                        {
                            string daytext = textBlock.Text;
                            DateTime selectedDate;
                            if (textBlock.Text == "Today")
                            {
                                daytext = DateTime.Today.Date.ToString();
                            }
                            else if (textBlock.Text == "Tomorrow")
                            {
                                daytext = DateTime.Today.AddDays(+1).ToString();
                            }
                            else if (textBlock.Text == "Yesterday")
                            {
                                daytext = DateTime.Today.AddDays(-1).ToString();
                            }


                            if (DateTime.TryParse(daytext, out selectedDate))
                            {

                                if (selectedButton == SummaryButton)
                                {
                                    ShowSummaryGraph(locationname, selectedDate);
                                    DateForDataContent.Content = selectedDate.ToString("M");

                                }
                                else if (selectedButton == HoursButton)
                                {
                                    var hourly = new Hourly();
                                    hourly.Date = selectedDate;
                                    DataContent.Content = hourly;
                                    DateForDataContent.Content = selectedDate.ToString("M");

                                }
                                else if (selectedButton == MoreDetailsButton)
                                {
                                    var moreDetails = new MoreDetails();
                                    moreDetails.Date = selectedDate;
                                    DataContent.Content = moreDetails;
                                    DateForDataContent.Content = selectedDate.ToString("M");

                                }
                                else
                                {
                                    ShowSummaryGraph(locationname, selectedDate);
                                    DateForDataContent.Content = selectedDate.ToString("M");
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
        private void HighlightButton(Button button)
        {
            if (selectedButton != null)
            {
                selectedButton.Style = (Style)FindResource("NavButtonStyle");
            }

            button.Style = (Style)FindResource("SelectedNavButtonStyle");
            selectedButton = button;
        }
        private void SummaryButton_Click(object sender, RoutedEventArgs e)
        {
            SummaryButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A354F"));
            HoursButton.Background = new SolidColorBrush(Colors.Transparent);
            MoreDetailsButton.Background = new SolidColorBrush(Colors.Transparent);
            HighlightButton(SummaryButton);            
            ShowSummaryGraph(locationname, DateTime.Today);

        }

        private async void HoursButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(HoursButton);
            var hourly = new Hourly();
            HoursButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A354F"));
            SummaryButton.Background = new SolidColorBrush(Colors.Transparent);
            MoreDetailsButton.Background = new SolidColorBrush(Colors.Transparent);
            await hourly.hourlydata(locationname, DateTime.Today);
            DateForDataContent.Content = DateTime.Today.ToString("M");
            DataContent.Content = hourly;
        }

        private  async void MoreDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(MoreDetailsButton);
            var moreDetails = new MoreDetails();
            MoreDetailsButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A354F")); // You can choose any color you prefer
            HoursButton.Background = new SolidColorBrush(Colors.Transparent);
            SummaryButton.Background = new SolidColorBrush(Colors.Transparent);
            await moreDetails.Suggestionsforday(locationname, DateTime.Today);
            DateForDataContent.Content = DateTime.Today.ToString("M");
            DataContent.Content = moreDetails;
        }
        private void ShowSummaryGraph(string location, DateTime date) //Avinash
        {
            var summaryGraph = SummaryGraph(location, date);
            if (summaryGraph != null)
            {
                DataContent.Content = summaryGraph;
                DateForDataContent.Content = date.ToString("M");
            }
        }
        public void RecentContainer()
        {
            try
            {
                // Clear the container
                RecentContent.Content = null;
                List<Location> locations = readRecentLocations();

                // Ensure there are locations to display
                if (locations == null || locations.Count == 0)
                {
                    return;
                }

                var recentLocations = locations.Take(3).ToList();

                StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

                foreach (var item in recentLocations)
                {
                    // Create a new instance of RecentLocation for each location
                    RecentLocation recentLocation = new RecentLocation();
                    recentLocation.TabInfo(item.LocationID, item.LocationName, item.iconlabel, item.temperature, item.IsPrimary);

                    // recentLocation.LocationSelected += RecentLocation_LocationSelected;
                    recentLocation.LocationClicked += (sender, e) => SelectLocation(e.LocationName);

                    // recentLocation.LocationRemoved += RecentLocation_LocationRemoved;
                    if (!item.IsPrimary)
                    {
                        recentLocation.RemoveButtonClicked += (sender, e) => RemoveLocation(item.LocationName);
                    }

                    panel.Children.Add(recentLocation);
                }

                // Set the ContentControl's content to the StackPanel
                RecentContent.Content = panel;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while displaying recent locations: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectLocation(string locationName)
        {
            string baseLocationName = locationName.Split(',')[0].Trim();

            List<Location> locations = readRecentLocations();
            var existingLocation = locations.FirstOrDefault(loc => loc.LocationName.Split(',')[0].Trim().Equals(baseLocationName, StringComparison.OrdinalIgnoreCase));

            if (existingLocation != null)
            {
                CityComboBox.Text = existingLocation.LocationName;
                LocationName.Content = existingLocation.LocationName;
                // Perform any other necessary actions with the existing location
            }
        }
        private void RecentLocation_LocationRemoved(object sender, LocationEventArgs e)
        {
            RemoveLocation(e.LocationName);
        }

        private void RemoveLocation(string locationName)
        {
            List<Location> locations = readRecentLocations();

            string baseLocationName = locationName.Split(',')[0].Trim();
            Location locationToRemove = locations.FirstOrDefault(loc =>
                  loc.LocationName.Split(',')[0].Trim().Equals(baseLocationName, StringComparison.OrdinalIgnoreCase));

           // Location locationToRemove = locations.SingleOrDefault(loc => loc.LocationName == locationName);

            if (locationToRemove != null)
            {
                locations.Remove(locationToRemove);

                // Serialize the updated list to JSON format
                string jsonData = JsonConvert.SerializeObject(locations, Formatting.Indented);

                // Write the JSON data to a file
                try
                {
                    System.IO.File.WriteAllText("RecentWeatherData.json", jsonData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while writing to the file: " + ex.Message);
                }

                // Refresh the recent locations display
                RecentContainer();
            }
        }
        private async void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedLocation = CityComboBox.Text;
                if (!string.IsNullOrWhiteSpace(selectedLocation))
                {
                    recentLocations.Clear();
                    await RecentLocations(selectedLocation, true);
                    RecentContainer();       
                    locationname = selectedLocation;
                    PrimaryButton.Visibility = Visibility.Hidden;
                    PrimaryIcon.Visibility = Visibility.Visible;
                    await DisplayErrorhandlingMessage($"{selectedLocation} has been set as the primary location", Colors.Green,Colors.LimeGreen,true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while setting primary location: " + ex.Message);
            }
        }

        private async void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) //Avinash
        {
            if (CityComboBox.SelectedIndex >= 0)
            {
             
                var selectedItem = CityComboBox.SelectedItem.ToString();
                SearchTextBox.Text = "";
                locationname = selectedItem;
                LocationName.Content = locationname;
                CityComboBox.Text = selectedItem;
                await LocationSearched();
                recentLocations = readRecentLocations();
                foreach (Location primary in recentLocations)
                {
                    if (primary.IsPrimary == true && primary.LocationName == selectedItem)
                    {
                        PrimaryButton.Visibility = Visibility.Hidden;
                        PrimaryIcon.Visibility = Visibility.Visible;
                    }
                    if (primary.IsPrimary == false && primary.LocationName == selectedItem)
                    {
                        PrimaryButton.Visibility = Visibility.Visible;
                        PrimaryIcon.Visibility = Visibility.Hidden;
                    }
                }
                RecentContainer();
            }
        }
        private async void Refresh_Click(object sender, RoutedEventArgs e) 
        {
            await Dailyweather(locationname);
            await WeeklyWeather(locationname);
            await LoadMap(locationname);
            recentLocations = readRecentLocations();
            foreach (Location primary in recentLocations)
            {
                if (primary.IsPrimary == true && primary.LocationName == locationname)
                {
                    PrimaryButton.Visibility = Visibility.Hidden;
                    PrimaryIcon.Visibility = Visibility.Visible;
                }
                if (primary.IsPrimary == false && primary.LocationName == locationname)
                {
                    PrimaryButton.Visibility = Visibility.Visible;
                    PrimaryIcon.Visibility = Visibility.Hidden;
                }
            }
            RecentContainer();
            CityComboBox.Items.Clear();
            foreach (var item in recentLocations)
            {
                CityComboBox.Items.Add(item.LocationName);
            }
            CityComboBox.Text = locationname;
            LocationName.Content = locationname;
            CityComboBox.SelectedItem = locationname;
            ShowSummaryGraph(locationname, DateTime.Today);
        }
        public async Task<bool> TextValidation(string location) //Avinash
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                await DisplayErrorhandlingMessage("Please enter a location to search.", Colors.Red, Colors.Orange,true);
                return false; // Invalid input if the location is empty or whitespace
            }

            bool isValidCity = await IsValidLocationAsync(location);

            if (isValidCity)
            {
                return true; // The input is a valid city name
            }
            else
            {
                var coordinates = await GetCoordinatesFromGeoNamesAsync(location);

                if (coordinates.HasValue)
                {
                    return true; // The input is a valid coordinate format
                }
                else
                {
                    await DisplayErrorhandlingMessage("The location could not be found.Try searching for a location/city.", Colors.Red,Colors.Orange, true);
                    return false; // Invalid location
                }
            }
        }

    }
}


public class geonames
{
//    public string adminCode1 { get; set; }
    public string lng { get; set; }
//    public int geonameId { get; set; }
    public string postalCode { get; set; }
//    public string toponymName { get; set; }
//    public string countryId { get; set; }
//    public string fcl { get; set; }
//    public string countryCode { get; set; }
    public string name { get; set; }
//    public string fclName { get; set; }
 //   public AdminCodes1 adminCodes1 { get; set; }
    public string countryName { get; set; }
 //   public string fcodeName { get; set; }
    public string adminName1 { get; set; }
    public string lat { get; set; }
 //   public string fcode { get; set; }
}
public class geonamesResult
{
//    public int totalResultsCount { get; set; }
    public List<geonames> geonames { get; set; }
}


public class AdminCodes1
{
 //   public string ISO3166_2 { get; set; }
}

public class RootObject
{
 //   public int totalResultsCount { get; set; }
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
//    public string adminCode1 { get; set; }
//    public string adminCode2 { get; set; }
//    public string adminCode3 { get; set; }
    public string adminName1 { get; set; }
    public string adminName2 { get; set; }
    public string adminName3 { get; set; }
    public string countryCode { get; set; }
//    public string ISO3166_2 { get; set; }
//    public double lat { get; set; }
//  public double lng { get; set; }
    public string placeName { get; set; }
    public string postalCode { get; set; }
}
    
