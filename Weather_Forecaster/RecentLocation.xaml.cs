using System;
using System.Collections.Generic;
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

namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for RecentLocation.xaml
    /// </summary>
    public partial class RecentLocation : UserControl
    {
        public int LocationID { get; set; }
        public string Iconlabel;
        private static HttpClient httpClient = new HttpClient();

        public RecentLocation()
        {
            InitializeComponent();

        }

        public async void TabInfo(int locationid, String locationname, string icon, int temperature, bool primary)
        {
            try
            {
                // Extract the location name (before the comma, if any)
                int commaIndex = locationname.IndexOf(',');
                string labelText = commaIndex != -1 ? locationname.Substring(0, commaIndex) : locationname;

                // Assign the location name to LocName1
                LocName1.Content = labelText;
                Temp1.Content = $"{temperature}°C";

                // Load and set the weather icon
                if (icon != null)
                {
                    string iconUrl = $"http://openweathermap.org/img/wn/{icon}.png";
                    byte[] imageData = await httpClient.GetByteArrayAsync(iconUrl);
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageData))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        Icon1.Source = bitmap;
                        // Icon1.Source = new BitmapImage(new Uri(iconLabel, UriKind.RelativeOrAbsolute));


                    }
                }
                else
                {
                    Icon1.Source= null;
                }
                // Set the temperature
                if (primary)
                {
                    Remove.Visibility = Visibility.Collapsed;
                    HomeIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    Remove.Visibility = Visibility.Visible;
                    HomeIcon.Visibility = Visibility.Collapsed;
                }
                LocationID = locationid;
            }
            catch (Exception)
            {
                // Extract the location name (before the comma, if any)
                int commaIndex = locationname.IndexOf(',');
                string labelText = commaIndex != -1 ? locationname.Substring(0, commaIndex) : locationname;

                // Assign the location name to LocName1
                LocName1.Content = labelText;
                // Set the temperature

                Icon1.Source = null;
                Temp1.Content = $"{temperature}°C";
                if (primary)
                {
                    Remove.Visibility = Visibility.Collapsed;
                    HomeIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    Remove.Visibility = Visibility.Visible;
                    HomeIcon.Visibility = Visibility.Collapsed;
                }
                LocationID = locationid;

            }

        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // LocationSelected?.Invoke(this, new LocationEventArgs { LocationName = LocName1.Content.ToString() });
            LocationClicked?.Invoke(this, new LocationEventArgs { LocationName = LocName1.Content.ToString() });

        }
        private void Remove_click(object sender, RoutedEventArgs e)
        {
            // LocationRemoved?.Invoke(this, new LocationEventArgs { LocationName = LocName1.Content.ToString() });
            RemoveButtonClicked?.Invoke(this, new LocationEventArgs { LocationName = LocName1.Content.ToString() });

        }

        //  public event EventHandler<LocationEventArgs> LocationSelected;
        public event EventHandler<LocationEventArgs> LocationClicked;

        // public event EventHandler<LocationEventArgs> LocationRemoved;
        public event EventHandler<LocationEventArgs> RemoveButtonClicked;



    }
    public class LocationEventArgs : EventArgs
    {
        public string LocationName { get; set; }
    }
}
