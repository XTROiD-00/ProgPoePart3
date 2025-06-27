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
using System.Windows.Shapes;
using ProgPoePart3;

namespace ProgPoePart3
{
    /// <summary>
    /// Interaction logic for Logo.xaml
    /// </summary>
    public partial class Logo : Window
    {
        public Logo()
        {

            // Get the directory where your .exe is running
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Build the full path to your logo file
            string imagePath = System.IO.Path.Combine(baseDir, "Logo.png");

            // Load the image into the Image control
            logoImage.Source = new BitmapImage(new Uri(imagePath));
        }
    }
}