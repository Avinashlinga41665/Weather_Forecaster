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

namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for HorizontalTab.xaml
    /// </summary>
    public partial class HorizontalTab : UserControl
    {
        public HorizontalTab()
        {
            InitializeComponent();
        }
        public void TabInfo(DateTime date,string MinTemperature,string MaxTemperature,string Description, ImageSource icon)
        {
           WeatherDate.Text= Convert.ToString(date);
           MinDayTemperature.Text= MinTemperature;
           MaxDayTemperature.Text = MaxTemperature; 
           DayDescription.Text= Description;
            Icon.Source = icon;
        }
    }
}
