
using NAudio.Wave;

namespace Spritzgusslogik;

public class Microphone
{
    private WaveInEvent waveIn;
    private Int16[] values;

    private Boolean muted = true;

    public Microphone()
    {
        waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(rate: 44100, bits: 16, channels: 1),
            BufferMilliseconds = 20
        };
        waveIn.DataAvailable += WaveIn_DataAvailable;
    }

    public void Start()
    {
        try
        {
            waveIn.StartRecording();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim StartRecording: " + ex.Message);
        }
        //waveIn.StartRecording();
        muted = false;
    }

    public void Stop()
    {
        waveIn.StopRecording();
        muted = true;
    }

    void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        values = new Int16[e.Buffer.Length / 2];
        Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
    }

    public Boolean IsMuted()
    {
        return muted;
    }

    public String GetStandardMicrophoneName()
    {
        if (WaveIn.DeviceCount == 0)
        {
            return "No microphone found";
        }

        var caps = WaveIn.GetCapabilities(0);
        return caps.ProductName;
    }

    public Int16[] GetValues()
    {
        return values;
    }
}