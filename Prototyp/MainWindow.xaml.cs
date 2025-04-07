using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Prototyp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean isRunning = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void toggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.isRunning = !this.isRunning;

            String content;
            String backgroundColor;

            if (this.isRunning)
            {
                content = "Stop";
                backgroundColor = "#ff9d98";
            }
            else
            {
                content = "Start";
                backgroundColor = "#75bd33";
            }

            toggleButton.Content = content;
            toggleButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
        }
    }
}