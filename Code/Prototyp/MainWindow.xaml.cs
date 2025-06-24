using System.Runtime.InteropServices.Marshalling;
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
using Microsoft.VisualBasic;
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
        private SignalIdentifier identifier;

        private bool _isProcessing = false;


        //Cooldown, um AudioWiederholung zu verhindern
        TimeSpan minDelay = TimeSpan.FromSeconds(2);
        DateTime lastRecognized = DateTime.MinValue;


        public MainWindow()
        {
            InitializeComponent();

            microphone = new Microphone();
            signalDrawer = new SignalDrawer(PlotView);
            signalGuiUpdater = new SignalGuiUpdater(microphone, signalDrawer);
            _history = new History();
            identifier = new SignalIdentifier();

            microphoneName.Text = microphone.GetStandardMicrophoneName();

            identifier.DigitRecognized += OnDigitRecognized;

            microphone.AudioReceived += OnAudioReceived;
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
                signalDrawer.ClearWaveform();
            }

            ToggleButton.Content = content;
            ToggleButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            microphone.Stop();
        }

        public void UpdateCurrentCommand(int value, double confidence)
        {
            AddToHistory(value);
            SetCurrentCommand(value);
            SetCommandConfidence(confidence * 100);
        }

        private void SetCurrentCommand(int value)
        {
            commandText.Text = value.ToString();
        }

        private void SetCommandConfidence(double confidence)
        {
            commandConfidence.Text = confidence.ToString("0") + "%";
            UpdateConfidenceColor(confidence);
        }

        private void UpdateConfidenceColor(double confidence)
        {
            String color;

            if (confidence < 50)
            {
                color = "#cf241b";
            }
            else if (confidence < 80)
            {
                color = "#de8a0b";
            }
            else
            {
                color = "#75bd33";
            }

            commandConfidence.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        /// <summary>
        /// adds new entry to history and displays it on gui
        /// </summary>
        /// <param name="digit">digit command</param>
        private void AddToHistory(int digit)
        {
            _history.AddEntry(digit);
            StackPanel horizontalPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TextBlock timeText = new TextBlock
            {
                Text = _history.entries.Last().time.ToString("HH:mm:ss"),
                FontSize = 16,
                Foreground = Brushes.DimGray
            };

            Rectangle spacer = new Rectangle
            {
                Width = 5
            };

            TextBlock valueText = new TextBlock
            {
                Text = digit.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            };

            horizontalPanel.Children.Add(timeText);
            horizontalPanel.Children.Add(spacer);
            horizontalPanel.Children.Add(valueText);

            HistoryPanel.Children.Insert(0, horizontalPanel);
        }

        /// <summary>
        /// if audio is received the recognititon process is started
        /// </summary>
        /// <param name="pcm">input audio signal</param>
        private void OnAudioReceived(Int16[] pcm)
        {
            if (_isProcessing || (DateTime.Now - lastRecognized) < minDelay)
                return;

            _isProcessing = true;

            Task.Run(() =>
            {
                try
                {
                    identifier.Recognition(pcm);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    _isProcessing = false;
                }
            });
        }

        /// <summary>
        /// If digit is recognized gui should be updated
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">Digit Arguments = Digit & Command</param>
        private void OnDigitRecognized(object sender, DigitRecognizedEventArgs e)
        {
            lastRecognized = DateTime.Now;
            Dispatcher.Invoke(() =>
            {
                UpdateCurrentCommand(e.Digit, e.Confidence);
            });
        }
    }
}