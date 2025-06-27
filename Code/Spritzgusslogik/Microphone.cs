using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Timers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Spritzgusslogik
{
    /// <summary>
    /// Represents a microphone interface with support for dynamic voice activation.
    /// </summary>
    public class Microphone
    {
        /// <summary>
        /// Sample rate for audio input.
        /// </summary>
        public const int SampleRate = 44100;

        private WaveInEvent waveIn;
        private Int16[] values;

        private bool muted = true;

        private short[]? lastAudioData;

        // Dynamic voice activation parameters
        private const float VoiceStartThreshold = 0.03f;
        private const float VoiceEndThreshold = 0.015f;
        private const int SilenceDurationMs = 500;

        private bool isRecording = false;
        private DateTime lastVoiceTime = DateTime.MinValue;
        private List<short> activeRecordingBuffer = new();

        private readonly IAudioDeviceManager audioDeviceManager;
        private readonly IAudioRecorder audioRecorder;

        /// <summary>
        /// Initializes a new instance of the <see cref="Microphone"/> class.
        /// </summary>
        public Microphone(IAudioDeviceManager deviceManager, IAudioRecorder recoder)
        {
            audioDeviceManager = deviceManager;
            audioRecorder = recoder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Microphone"/> class with the default audio device manager.
        /// </summary>
        public Microphone() : this(new NAudioDeviceManager(), new NAudioRecorder(new WaveInEvent
        {
            DeviceNumber = 0,
            WaveFormat = new WaveFormat(rate: SampleRate, bits: 16, channels: 1),
            BufferMilliseconds = 30
        }))
        {
            var waveInEvent = ((NAudioRecorder)audioRecorder).WaveIn;
            waveInEvent.DataAvailable += WaveIn_DataAvailable;
        }

        /// <summary>
        /// Starts the audio recording.
        /// </summary>
        public void Start()
        {
            try
            {
                audioRecorder.StartRecording();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while starting recording: " + ex.Message);
            }
            muted = false;
        }

        /// <summary>
        /// Stops the audio recording.
        /// </summary>
        public void Stop()
        {
            audioRecorder.StopRecording();
            muted = true;
        }

        /// <summary>
        /// Checks whether the microphone is muted.
        /// </summary>
        /// <returns><c>true</c> if muted; otherwise, <c>false</c>.</returns>
        public bool IsMuted()
        {
            return muted;
        }

        /// <summary>
        /// Retrieves the name of the default microphone device.
        /// </summary>
        /// <returns>The name of the microphone, or a message if none is found.</returns>
        public string GetStandardMicrophoneName()
        {
            if (audioDeviceManager.DeviceCount == 0)
            {
                return "No microphone found";
            }

            var caps = audioDeviceManager.GetCapabilities(0);
            return caps.ProductName;
        }

        /// <summary>
        /// Returns the last captured audio sample values.
        /// </summary>
        /// <returns>An array of 16-bit audio sample values.</returns>
        public Int16[] GetValues()
        {
            return values;
        }

        /// <summary>
        /// Event triggered when valid audio input is received.
        /// </summary>
        public event Action<Int16[]>? AudioReceived;

        /// <summary>
        /// Handles incoming audio buffers, performs voice activity detection,
        /// and triggers <see cref="AudioReceived"/> when voice input is complete.
        /// </summary>
        /// <param name="sender">The source of the audio input event.</param>
        /// <param name="e">Event arguments containing audio buffer data.</param>
        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
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

        /// <summary>
        /// Calculates the root mean square (RMS) value of an audio sample array.
        /// </summary>
        /// <param name="samples">The array of audio sample data.</param>
        /// <returns>The RMS value representing the signal volume.</returns>
        internal float CalculateRMS(short[] samples)
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

    public interface IAudioDeviceManager
    {
        int DeviceCount { get; }

        /// <summary>
        /// Gets the capabilities of a specific audio input device.
        /// </summary>
        /// <param name="deviceNumber">The index of the audio device to query.</param>
        /// <returns>The capabilities of the audio device.</returns>
        WaveInCapabilities GetCapabilities(int deviceNumber);
    }

    public class NAudioDeviceManager : IAudioDeviceManager
    {
        public int DeviceCount => WaveIn.DeviceCount;

        public WaveInCapabilities GetCapabilities(int deviceNumber)
        {
            return WaveIn.GetCapabilities(deviceNumber);
        }
    }

    public interface IAudioRecorder
    {
        void StartRecording();
        void StopRecording();
    }

    public class NAudioRecorder : IAudioRecorder
    {
        private readonly WaveInEvent waveIn;
        public WaveInEvent WaveIn => waveIn;

        public NAudioRecorder(WaveInEvent waveIn)
        {
            this.waveIn = waveIn;
        }

        public void StartRecording() => waveIn.StartRecording();
        public void StopRecording() => waveIn.StopRecording();
    }
}
