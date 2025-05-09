
using NAudio.Wave;

namespace Spritzgusslogik;

public class Microphone
{
    private WaveInEvent waveIn;
    private Int16[] values;

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

    public void start()
    {
        waveIn.StartRecording();
    }

    public void stop()
    {
        waveIn.StopRecording();
    }

    void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        values = new Int16[e.Buffer.Length / 2];
        Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);
    }

    public Int16[] getValues()
    {
        return values;
    }
}