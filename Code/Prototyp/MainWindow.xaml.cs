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
    /// Interaction logic for the main application window.
    /// Manages GUI updates, microphone input, signal recognition, and history.
    /// </summary>
    public partial class MainWindow : Window
    {
        private Microphone microphone;

        private SignalDrawer signalDrawer;
        private SignalGuiUpdater signalGuiUpdater;
        private History _history;
        private SignalIdentifier identifier;

        private bool _isProcessing = false;

        // Zustand fuer den gefuehrten Referenz-Aufnahmemodus
        private const int ReferenceRepetitions = 3;
        private bool _recordingReferences = false;
        private int _referenceDigit = 1;
        private int _referenceRepetition = 1;


        //Cooldown, um AudioWiederholung zu verhindern
        TimeSpan minDelay = TimeSpan.FromSeconds(2);
        DateTime lastRecognized = DateTime.MinValue;

        /// <summary>
        /// Constructor: Initializes the main window and its components.
        /// </summary>
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


        /// <summary>
        /// Handles start/stop toggle button click for microphone recording.
        /// </summary>
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

        /// <summary>
        /// Ensures the microphone is stopped when the window closes.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            microphone.Stop();
        }

        /// <summary>
        /// Updates current command output on the GUI.
        /// </summary>
        /// <param name="value">Recognized digit</param>
        /// <param name="confidence">Recognition confidence (0–1)</param>
        public void UpdateCurrentCommand(int value, double confidence)
        {
            AddToHistory(value);
            SetCurrentCommand(value);
            SetCommandConfidence(confidence * 100);
        }

        /// <summary>
        /// Displays the recognized digit on screen.
        /// </summary>
        private void SetCurrentCommand(int value)
        {
            commandText.Text = value.ToString();
        }

        /// <summary>
        /// Displays and colors the recognition confidence on the GUI.
        /// </summary>
        private void SetCommandConfidence(double confidence)
        {
            commandConfidence.Text = confidence.ToString("0") + "%";
            UpdateConfidenceColor(confidence);
        }

        /// <summary>
        /// Changes the text color of the confidence label depending on confidence level.
        /// </summary>
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
        /// Adds a digit entry to the GUI and history log.
        /// </summary>
        /// <param name="digit">The recognized digit to be logged</param>
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
        /// Handles the audio input event and starts recognition asynchronously.
        /// </summary>
        /// <param name="pcm">PCM audio data from microphone</param>
        private void OnAudioReceived(Int16[] pcm)
        {
            if (_recordingReferences)
            {
                HandleReferenceRecording(pcm);
                return;
            }

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
        /// Starts the guided reference recording: the user is asked to say each digit
        /// several times; the recordings are stored and the references are rebuilt.
        /// </summary>
        private void RecordReferences_Click(object sender, RoutedEventArgs e)
        {
            if (_recordingReferences)
                return;

            _recordingReferences = true;
            _referenceDigit = 1;
            _referenceRepetition = 1;

            if (microphone.IsMuted())
            {
                microphone.Start();
                ToggleButton.Content = "Stop";
                ToggleButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff9d98"));
            }

            RecordReferencesButton.IsEnabled = false;
            UpdateReferenceStatus();
        }

        private void UpdateReferenceStatus()
        {
            referenceStatus.Text =
                $"Sprich Ziffer {_referenceDigit} – Aufnahme {_referenceRepetition}/{ReferenceRepetitions}";
        }

        /// <summary>
        /// Saves the current utterance as a reference sample and advances the guided flow.
        /// </summary>
        private void HandleReferenceRecording(Int16[] pcm)
        {
            if (_isProcessing)
                return;

            _isProcessing = true;

            Task.Run(() =>
            {
                try
                {
                    string samplesDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                        "SampleToFile", "WAV_samples"));
                    System.IO.Directory.CreateDirectory(samplesDir);

                    string fileName = $"own{_referenceRepetition}_{_referenceDigit}.wav";
                    audioConvert.SavePcmAsWav(pcm, System.IO.Path.Combine(samplesDir, fileName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    _isProcessing = false;
                }

                Dispatcher.Invoke(() =>
                {
                    _referenceRepetition++;
                    if (_referenceRepetition > ReferenceRepetitions)
                    {
                        _referenceRepetition = 1;
                        _referenceDigit++;
                    }

                    if (_referenceDigit > 9)
                    {
                        _recordingReferences = false;
                        RecordReferencesButton.IsEnabled = true;
                        referenceStatus.Text = "Aufnahmen fertig – Referenzen werden neu berechnet …";

                        Task.Run(() =>
                        {
                            try
                            {
                                identifier.RebuildReferenceSequences();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

                            Dispatcher.Invoke(() =>
                                referenceStatus.Text = "Referenzen aktualisiert. Du kannst jetzt testen.");
                        });
                    }
                    else
                    {
                        UpdateReferenceStatus();
                    }
                });
            });
        }

        /// <summary>
        /// Updates the GUI when a digit is recognized by the recognition module.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Digit recognition event arguments</param>
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