
using NAudio.Wave;

namespace Spritzgusslogik;


//Diese Klasse speichert die Audiodaten der Aufnahme in ein int16-Array

public class Microphone
{
    public const int SampleRate = 44100;

    private WaveInEvent waveIn;
    private Int16[] values;

    private Boolean muted = true;

    //damit Eingabe gleich groﬂ wie Samples
    private const int TargetSampleCount = SampleRate * 1;
    private List<short> buffer = new();
    public Microphone()
    {
        waveIn = new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(rate: SampleRate, bits: 16, channels: 1),
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


    //Benachrichtigung sobald neue Audiodatei
    //EventHandler
    public event Action<Int16[]>? AudioReceived;

    void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        values = new Int16[e.Buffer.Length / 2];
        Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

        //neue samples anh‰ngen
        buffer.AddRange(values);

        //wenn genug daten gesammelt -> verarbeiten
        if (buffer.Count >= TargetSampleCount)
        {
            var fixedBlock = buffer.Take(TargetSampleCount).ToArray();
            //Clear?
            buffer.RemoveRange(0, TargetSampleCount);

            AudioReceived?.Invoke(fixedBlock) ;
        }
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