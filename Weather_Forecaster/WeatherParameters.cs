using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather_Forecaster
{
    internal class WeatherParameters //Avinash
    {

        public class coord
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }
        public class weather
        {
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }
        public class main
        {
            public int aqi { get; set; }
            public double temp { get; set; }
            public double feels_like { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
            public double pressure { get; set; }
            public double humidity { get; set; }



        }
        public class components
        {
            //public double co { get; set; }
            //public double no { get; set; }
            //public double no2 { get; set; }
            //public double o3 { get; set; }
            //public double so2 { get; set; }
            //public double pm2_5 { get; set; }
            //public double pm10 { get; set; }
            //public double nh3 { get; set; }
        }
        public class wind
        {
            public double speed { get; set; }
            public double deg { get; set; }

        }
        public class clouds
        {
            public int all { get; set; }
        }
        public class sys
        {
            //public string country { get; set; }
            //public int sunrise { get; set; }
            //public int sunset { get; set; }


        }
        public class ListItem
        {
//            public int dt { get; set; }
            public main main { get; set; }
            public weather[] weather { get; set; }
            public clouds clouds { get; set; }
            public wind wind { get; set; }
            public int visibility { get; set; }
            public double pop { get; set; }
            public sys sys { get; set; }
            public DateTime dt_txt { get; set; }
 //           public components components { get; set; }
        }
        public class root
        {
//            public string name { get; set; }
//            public sys sys { get; set; }
            public int timezone { get; set; }
            public int visibility { get; set; }
//            public clouds clouds { get; set; }
            public wind wind { get; set; }
            public main main { get; set; }
            public weather[] weather { get; set; }
            public coord coord { get; set; }
 //           public DateTime dt_txt { get; set; }
            public ListItem[] list { get; set; }
//            public double aqi { get; set; }
//            public List<ListItem> history { get; set; }

            public long dt { get; set; }

        }
    }
}
