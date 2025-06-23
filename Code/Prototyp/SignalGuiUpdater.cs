using Spritzgusslogik;
using System.Windows.Threading;

class SignalGuiUpdater
{
    private Microphone microphone;
    private SignalDrawer signalDrawer;
    private short[]? latestAudioData;
    private DispatcherTimer updateTimer;

    public SignalGuiUpdater(Microphone microphone, SignalDrawer signalDrawer)
    {
        this.microphone = microphone;
        this.signalDrawer = signalDrawer;

        microphone.AudioReceived += Microphone_AudioReceived;

        updateTimer = new DispatcherTimer();
        updateTimer.Interval = TimeSpan.FromMilliseconds(30);
        updateTimer.Tick += UpdateTimer_Tick;
        updateTimer.Start();
    }

    private void Microphone_AudioReceived(object? sender, short[] audioData)
    {
        // Nur den neuesten Puffer speichern, nicht direkt UI updaten
        latestAudioData = audioData;
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        if (latestAudioData != null)
        {
            // Update UI mit dem neuesten Audio-Datenpuffer
            signalDrawer.UpdateWaveform(latestAudioData);
            latestAudioData = null; // Reset nach Update
        }
    }
}
