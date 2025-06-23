using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Timers;

namespace Spritzgusslogik
{
    public class Microphone
    {
        public const int SampleRate = 44100;

        private WaveInEvent waveIn;
        private Int16[] values;

        private Boolean muted = true;

        private short[]? lastAudioData;


        //dynamische Spracherkennung
        private const float VoiceStartThreshold = 0.02f; // Lautstaerkeschwelle zum Start
        private const float VoiceEndThreshold = 0.015f; // Lautstaerkeschwelle zum Stoppen
        private const int SilenceDurationMs = 500; // Zeit ohne Stimme, bis gestoppt wird

        private bool isRecording = false;
        private DateTime lastVoiceTime = DateTime.MinValue;
        private List<short> activeRecordingBuffer = new();


        public Microphone()
        {
            waveIn = new WaveInEvent
            {
                DeviceNumber = 0,
                WaveFormat = new WaveFormat(rate: SampleRate, bits: 16, channels: 1),
                BufferMilliseconds = 30
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



        //Benachrichtigung sobald neue Audiodatei
        //EventHandler
        public event Action<Int16[]>? AudioReceived;
        //Benachrichtigung sobald neue Audiodatei
        //EventHandler

        void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            values = new Int16[e.Buffer.Length / 2];
            Buffer.BlockCopy(e.Buffer, 0, values, 0, e.Buffer.Length);

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
}
