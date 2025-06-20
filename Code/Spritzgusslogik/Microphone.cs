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

        public Microphone()
        {
            try
            {
                var device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);

                capture = new WasapiCapture(device)
                {
                    WaveFormat = new WaveFormat(SampleRate, 1) // Mono, 16-bit
                };

                System.Diagnostics.Debug.WriteLine($"Capture Format: {capture.WaveFormat.Encoding}, {capture.WaveFormat.BitsPerSample} bit, {capture.WaveFormat.SampleRate} Hz, {capture.WaveFormat.Channels} Kanal(e)");

                capture.DataAvailable += OnDataAvailable;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim Initialisieren des Mikrofons: " + ex.Message);
            }
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

                // Kopiere in korrekter Reihenfolge aus Ringpuffer
                int startPos = (writePos) % BufferSize;
                int firstPartLen = BufferSize - startPos;

                Array.Copy(audioBuffer, startPos, output, 0, firstPartLen);
                if (startPos > 0)
                    Array.Copy(audioBuffer, 0, output, firstPartLen, startPos);

                AudioReceived?.Invoke(this, output);
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
}
