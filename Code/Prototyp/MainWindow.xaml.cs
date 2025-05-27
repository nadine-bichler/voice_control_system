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

using Spritzgusslogik;

namespace Prototyp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Microphone microphone;

        private SignalDrawer signalDrawer;
        private SignalGuiUpdater signalGuiUpdater;
        private History _history;

        public MainWindow()
        {
            InitializeComponent();

            microphone = new Microphone();
            signalDrawer = new SignalDrawer(PlotView);
            signalGuiUpdater = new SignalGuiUpdater(microphone, signalDrawer);
            _history = new History();

            microphoneName.Text = microphone.GetStandardMicrophoneName();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            String content;
            String backgroundColor;

            if (this.microphone.IsMuted())
            {
                content = "Stop";
                backgroundColor = "#ff9d98";
                microphone.Start();
            }
            else
            {
                content = "Start";
                backgroundColor = "#75bd33";
                microphone.Stop();
            }

            ToggleButton.Content = content;
            ToggleButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            microphone.Stop();
        }

        // TODO: Objektorientierte History einbauen
        private void AddToHistory(int value)
        {
            var lastEntry = _history.entries.Last();
            StackPanel horizontalPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock timeText = new TextBlock
            {
                Text = lastEntry.time.ToString("HH:mm:ss"),
                FontSize = 16,
                Foreground = Brushes.DimGray
            };

            Rectangle spacer = new Rectangle
            {
                Width = 5 
            };

            TextBlock valueText = new TextBlock
            {
                Text = lastEntry.command.ToString(),
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