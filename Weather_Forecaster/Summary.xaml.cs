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
namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for Summary.xaml
    /// </summary>
    public partial class Summary : UserControl
    {
        public Summary()
        {
            InitializeComponent();
        }

        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        public  Summary(Dictionary<DateTime, int> temperatures)
        {
            InitializeComponent();
            var values = new ChartValues<int>();
            var labels = new List<string>();
            var pointColors = new List<System.Windows.Media.Color>();

            foreach (var temp in temperatures)
            {
                values.Add(temp.Value);
                labels.Add(temp.Key.ToString("HH:mm"));
            }

            var series = new LineSeries
            {
                Title = "Temperature",
                Values = values,
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 10,
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.Transparent, // Set fill color to transparent
                Stroke = System.Windows.Media.Brushes.Black, // Set stroke color for the line
                StrokeThickness = 2 // Set line thickness
            };

            // Create custom point template with different colors for each point
            series.PointGeometry = DefaultGeometries.Circle;
            // Set point color based on temperature
            foreach (var temp in temperatures)
            {
                if (temp.Value > 25)
                {
                    series.PointForeground = new SolidColorBrush(System.Windows.Media.Colors.Red);
                }
                else if (temp.Value >= 20 && temp.Value <= 25)
                {
                    series.PointForeground = new SolidColorBrush(System.Windows.Media.Colors.Orange);
                }
                else
                {
                    series.PointForeground = new SolidColorBrush(System.Windows.Media.Colors.Blue);
                }
            }


            chart.Series = new SeriesCollection { series };

            chart.AxisX.Add(new Axis
            {
                Title = "Time",
                Labels = labels,
                ShowLabels = true, // Show axis labels
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false } // Disable gridlines
            });

            chart.AxisY.Add(new Axis
            {
                Title = "Temperature (°C)",
                Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false } // Disable gridlines
            });
        }
        private System.Windows.Media.Color GetPointColor(int temperature)
        {
            if (temperature > 25)
                return System.Windows.Media.Color.FromRgb(255, 0, 0); // Red
            else if (temperature >= 20 && temperature <= 25)
                return System.Windows.Media.Color.FromRgb(255, 165, 0); // Orange
            else
                return System.Windows.Media.Color.FromRgb(0, 0, 255); // Blue
        }

    }
}
