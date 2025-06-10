using OxyPlot.Series;
using OxyPlot.Wpf;
using OxyPlot;

class SignalDrawer
{
    private const int SampleRate = 44100;
    private const double DisplaySeconds = 0.5;
    private const int MaxSamples = (int)(SampleRate * DisplaySeconds);

    private PlotModel plotModel;
    private LineSeries waveformSeries;
    private OxyPlot.Axes.LinearAxis timeAxis;

    private double[] amplitudeBuffer = new double[MaxSamples];
    private int writePosition = 0;

    public SignalDrawer(PlotView plotView)
    {
        plotModel = new PlotModel { Title = "Zeitsignal" };

        timeAxis = new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Bottom,
            Title = "Zeit (s)",
            Minimum = 0,
            Maximum = DisplaySeconds,
            IsZoomEnabled = false,
            IsPanEnabled = false
        };

        plotModel.Axes.Add(timeAxis);

        plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
        {
            Position = OxyPlot.Axes.AxisPosition.Left,
            Title = "Amplitude",
            Minimum = -1,
            Maximum = 1,
            IsZoomEnabled = false,
            IsPanEnabled = false
        });

        waveformSeries = new LineSeries
        {
            Color = OxyColors.DarkRed,
            StrokeThickness = 1
        };

        plotModel.Series.Add(waveformSeries);
        plotView.Model = plotModel;
    }

    public void UpdateWaveform(short[] newSamples)
    {
        if (newSamples == null || newSamples.Length == 0) return;

        // Kopiere neue Samples in Buffer (FIFO-Mechanik)
        int copyCount = Math.Min(newSamples.Length, MaxSamples);

        // Shift alte Samples nach links, um Platz für neue zu schaffen
        Array.Copy(amplitudeBuffer, copyCount, amplitudeBuffer, 0, MaxSamples - copyCount);

        // Neue Samples ans Ende schreiben
        for (int i = 0; i < copyCount; i++)
        {
            amplitudeBuffer[MaxSamples - copyCount + i] = newSamples[i] / 32768.0;
        }

        // Aktualisiere PlotPoints
        waveformSeries.Points.Clear();

        for (int i = 0; i < MaxSamples; i++)
        {
            double time = i / (double)SampleRate;
            waveformSeries.Points.Add(new DataPoint(time, amplitudeBuffer[i]));
        }

        plotModel.InvalidatePlot(true);
    }

    public void ClearWaveform()
    {
        Array.Clear(amplitudeBuffer, 0, amplitudeBuffer.Length);
        waveformSeries.Points.Clear();
        plotModel.InvalidatePlot(true);
    }
}
