using Spritzgusslogik;
using System.Windows.Threading;

/// <summary>
/// Updates the waveform display with real-time audio data.
/// Uses a DispatcherTimer to periodically fetch microphone samples and draw them on the UI.
/// </summary>
class SignalGuiUpdater
{
    private DispatcherTimer timer;

    private Microphone microphone;
    private SignalDrawer signalDrawer;

    /// <summary>
    /// Constructor for SignalGuiUpdater.
    /// </summary>
    /// <param name="microphone">Reference to the microphone input object.</param>
    /// <param name="signalDrawer">UI component used to render the waveform.</param>
    public SignalGuiUpdater(Microphone microphone, SignalDrawer signalDrawer)
    {
        this.microphone = microphone;
        this.signalDrawer = signalDrawer;

        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    /// <summary>
    /// Handles the timer tick to update the waveform.
    /// If the microphone is not muted, retrieves current values and updates the signal drawer.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Event arguments (unused).</param>
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (microphone.IsMuted())
        {
            return;
        }

        var values = microphone.GetValues();
        if (values != null)
            signalDrawer.UpdateWaveform(values);
    }

}
