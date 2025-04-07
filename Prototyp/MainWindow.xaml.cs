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

            // Dummy Daten für die History
            AddToHistory(2);
            AddToHistory(6);
            AddToHistory(1);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
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

            ToggleButton.Content = content;
            ToggleButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
        }

        private void AddToHistory(int value)
        {
            StackPanel horizontalPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            String currentTime = DateTime.Now.ToString("HH:mm:ss");

            TextBlock timeText = new TextBlock
            {
                Text = currentTime,
                FontSize = 16,
                Foreground = Brushes.DimGray
            };

            Rectangle spacer = new Rectangle
            {
                Width = 5 
            };

            TextBlock valueText = new TextBlock
            {
                Text = value.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            };

            horizontalPanel.Children.Add(timeText);
            horizontalPanel.Children.Add(spacer);
            horizontalPanel.Children.Add(valueText);

            HistoryPanel.Children.Add(horizontalPanel);
        }
    }
}