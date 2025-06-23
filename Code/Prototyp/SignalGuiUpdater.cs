using Spritzgusslogik;
using System.Windows.Threading;

class SignalGuiUpdater
{
    private DispatcherTimer timer;

    private Microphone microphone;
    private SignalDrawer signalDrawer;

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
