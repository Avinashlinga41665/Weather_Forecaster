using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
using System.Drawing;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using System.Net.Http;
using System.Windows.Media.Animation;
namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary : UserControl
    {
        private List<RainVideo> rainVideos = new List<RainVideo>();

        public Summary()
        {
            InitializeComponent();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public Summary(Dictionary<DateTime, int> temperatures, Dictionary<DateTime, int> RainPercentage) : this() //Avinash
        {
            InitializeComponent();

            var Tempvalues = new ChartValues<int>();
            var labels = new List<string>();
            foreach (var temp in temperatures)
            {
                Tempvalues.Add(temp.Value);

                // Include time and rain percentage in the label
                if (RainPercentage.TryGetValue(temp.Key, out int rainPercentage))
                {
                    labels.Add($"💧{rainPercentage}% \n {temp.Key.ToString("hh:mm tt")}");
                }
                else
                {
                    labels.Add(temp.Key.ToString("hh:mm tt"));
                }
            }

            var seriesTemperature = new LineSeries
            {
                Title = "Temperature",
                Values = Tempvalues,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                DataLabels = true,
                Fill = Brushes.Transparent,
                Stroke = Brushes.White,
                StrokeThickness = 2,
            };

            // Add the series to the chart
            chart.Series = new SeriesCollection { seriesTemperature };

            // Add X-axis
            chart.AxisX.Add(new Axis
            {
                Title = "Time",
                Labels = labels,
                Foreground = Brushes.White,
                ShowLabels = true,
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
            });

            // Add X-axis border
            chart.AxisX[0].Separator.Stroke = Brushes.White;
            chart.AxisX[0].Separator.StrokeThickness = 1;

            // Add Y-axis for temperature
            chart.AxisY.Add(new Axis
            {
                Title = "Temperature (°C)",
                LabelFormatter = value => value + "°C",
                Foreground = Brushes.White,
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false }
            });

            // Add rain videos for each section based on rain percentage
            for (int i = 0; i < temperatures.Count; i++)
            {
                var temp = temperatures.ElementAt(i);
                if (RainPercentage.TryGetValue(temp.Key, out int rainPercentage))
                {
                    if (rainPercentage > 95)
                    {
                        AddRainVideo();
                    }
                }
            }

            PositionRainVideos(RainPercentage);
        }

        private void AddRainVideo() //Avinash
        {
            RainVideo rainVideo = new RainVideo(); // Instantiate your RainVideo UserControl

            // Store the rain video and its index for positioning later
            rainVideos.Add(rainVideo);

            overlayCanvas.Children.Add(rainVideo); // Add RainVideo UserControl to overlayCanvas
        }

        private void PositionRainVideos(Dictionary<DateTime, int> RainPercentage) //Avinash
        {
            if (chart.AxisX[0].Labels == null || chart.AxisX[0].Labels.Count == 0)
                return;

            double step = (800) / (chart.AxisX[0].Labels.Count - 1);

            for (int i = 0; i < rainVideos.Count; i++)
            {
                double xPos = 20 + step * FindIndexForRainPercentageGreaterThan95(RainPercentage, i);
                RainVideo rainVideo = rainVideos[i];
                Canvas.SetLeft(rainVideo, xPos + 10);
                Canvas.SetTop(rainVideo, 20);
            }
        }

        private int FindIndexForRainPercentageGreaterThan95(Dictionary<DateTime, int> RainPercentage, int startIndex) //Avinash
        {
            for (int i = startIndex; i < RainPercentage.Count; i++)
            {
                if (RainPercentage.ElementAt(i).Value > 95)
                {
                    return i+1;
                }
            }
            return -1;
        }

    }
}
