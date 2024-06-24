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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Weather_Forecaster
{
    /// <summary>
    /// Interaction logic for RainVideo.xaml
    /// </summary>
    public partial class RainVideo : UserControl
    {
        private Random random = new Random();
        private List<Label> raindrops = new List<Label>();
        public RainVideo()
        {
            InitializeComponent();
            StartAnimation();
        }
        private void StartAnimation() //Avinash
        {
            raindrops.Add(drop1);
            raindrops.Add(drop2);
            raindrops.Add(drop3);
            raindrops.Add(drop4);
            raindrops.Add(drop5);
            raindrops.Add(drop6);
            raindrops.Add(drop7);
            raindrops.Add(drop8);
            raindrops.Add(drop9);
            foreach (Label drop in raindrops)
            {
                TranslateTransform transform = new TranslateTransform();
                drop.RenderTransform = transform;
            }

            foreach (Label drop in raindrops)
            {
                double duration = random.Next(2, 6); // Randomize duration
                double endPosition = this.ActualHeight + 250; // End position below the UserControl

                // Create the animation
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = -drop.ActualHeight, // Start above the UserControl
                    To = endPosition,
                    Duration = TimeSpan.FromSeconds(duration),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                // Set the target property and start the animation
                TranslateTransform transform = drop.RenderTransform as TranslateTransform;
                transform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

    }
}
