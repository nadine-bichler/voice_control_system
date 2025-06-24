///
/// @file    Microphone
/// @brief   Methods for Microphone and Input
/// @author  Benz Jan, Bichler Nadine
/// @date    24.06.2025
///
///

using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Timers;

namespace Spritzgusslogik
{
    /// <summary>
    /// instanciates a microphone
    /// </summary>
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


        /// <summary>
        /// creates a new microphone
        /// </summary>
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

        /// <summary>
        /// starts recording
        /// </summary>
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

        /// <summary>
        /// ends recording
        /// </summary>
        public void Stop()
        {
            waveIn.StopRecording();
            muted = true;
        }

        /// <summary>
        /// mutes the microphone
        /// </summary>
        /// <returns></returns>
        public Boolean IsMuted()
        {
            return muted;
        }

        /// <summary>
        /// gets standard microphone name
        /// </summary>
        /// <returns>microphone name</returns>
        public String GetStandardMicrophoneName()
        {
            if (WaveIn.DeviceCount == 0)
            {
                return "No microphone found";
            }

            var caps = WaveIn.GetCapabilities(0);
            return caps.ProductName;
        }

        /// <summary>
        /// gets input from recording
        /// </summary>
        /// <returns>input from recording</returns>
        public Int16[] GetValues()
        {
            return values;
        }


        /// <summary>
        /// Event for new audio input
        /// </summary>
        public event Action<Int16[]>? AudioReceived;
        

        /// <summary>
        /// EventHandler for new audio input
        /// if volume of a sample is bigger than startThreshold => record and if volume sinks under endThreshold => stop recording
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="e">input buffer</param>
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

        /// <summary>
        /// calculates average audio volume
        /// </summary>
        /// <param name="samples">audio signal from input</param>
        /// <returns>audio volume over sample durations</returns>
        private float CalculateRMS(short[] samples)
        {
            if (samples.Length == 0) return 0f;

            double sum = 0;
            foreach (short s in samples)
            {
                float norm = s / 32768f;
                sum += norm;
            }

            return (float)Math.Sqrt(sum / samples.Length);
        }
    }
}
