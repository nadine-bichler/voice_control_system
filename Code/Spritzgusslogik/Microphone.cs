using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Timers;

namespace Spritzgusslogik
{
    public class Microphone
    {
        public const int SampleRate = 44100;

        private WasapiCapture? capture;
        private WaveFileWriter? writer;
        private bool muted = true;

        private short[]? lastAudioData;

        private const int BufferSize = SampleRate / 2; // 0.5 Sekunden
        private short[] audioBuffer = new short[BufferSize];
        private int writePos = 0;
        private int samplesInBuffer = 0;

        // Event to notify when new audio data is available
        public event EventHandler<short[]>? AudioReceived;

        //dynaische Spracherkennung
        private const float VoiceStartThreshold = 0.025f;  // Lautstärkeschwelle zum Start
        private const float VoiceEndThreshold = 0.015f;   // Lautstärkeschwelle zum Stoppen
        private const int SilenceDurationMs = 500;        // Zeit ohne Stimme, bis gestoppt wird

        private bool isRecording = false;
        private DateTime lastVoiceTime = DateTime.MinValue;
        private List<short> activeRecordingBuffer = new();

   

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
            if (capture == null) return;

            try
            {
                capture.StartRecording();
                muted = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim StartRecording: " + ex.Message);
            }
        }

        public void Stop()
        {
            if (!muted && capture != null)
            {
                capture.StopRecording(); // Triggert OnRecordingStopped
                muted = true;
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            int samples = e.BytesRecorded / 2;
            short[] chunk = new short[samples];
            Buffer.BlockCopy(e.Buffer, 0, chunk, 0, e.BytesRecorded);

            // Schreibe Samples in Ringpuffer
            for (int i = 0; i < samples; i++)
            {
                audioBuffer[writePos] = chunk[i];
                writePos = (writePos + 1) % BufferSize;
            }

            samplesInBuffer = Math.Min(samplesInBuffer + samples, BufferSize);

            if (samplesInBuffer == BufferSize)
            {
                // Voller Puffer, sende 2 Sekunden Audio raus
                short[] output = new short[BufferSize];

        float rms = CalculateRMS(values);

        if (isRecording)
        {
            activeRecordingBuffer.AddRange(values);

            if (rms > VoiceEndThreshold)
            {
                lastVoiceTime = DateTime.Now;
            }

            if ((DateTime.Now - lastVoiceTime).TotalMilliseconds > SilenceDurationMs)
            {
                isRecording = false;

                if (activeRecordingBuffer.Count > 1000)
                {
                    AudioReceived?.Invoke(activeRecordingBuffer.ToArray());
                }

                activeRecordingBuffer.Clear();
            }
        }
        else
        {
            if (rms > VoiceStartThreshold)
            {
                isRecording = true;
                lastVoiceTime = DateTime.Now;
                activeRecordingBuffer.Clear();
                activeRecordingBuffer.AddRange(values);
            }
        }
    }

        public bool IsMuted()
        {
            return muted;
        }

        public string GetStandardMicrophoneName()
        {
            try
            {
                var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
                return device.FriendlyName;
            }
            catch
            {
                return "Kein Mikrofon gefunden";
            }
        }

        public short[]? GetValues()
        {
            return lastAudioData;
        }
    }

    private float CalculateRMS(short[] samples)
    {
        if (samples.Length == 0) return 0f;

        double sum = 0;
        foreach (short s in samples)
        {
            float norm = s / 32768f;
            sum += norm * norm;
        }

        return (float)Math.Sqrt(sum / samples.Length);
    }
}