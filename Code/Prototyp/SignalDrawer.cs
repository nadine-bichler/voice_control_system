using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;
using System.Linq;
using OxyPlot.Wpf;

namespace Prototyp
{
    // Diese Klasse zeichnet die Mikrofonsignale in ein Linien-Diagramm
    class SignalDrawer
    {

        private PlotModel plotModel;
        private LineSeries waveformSeries;

        public SignalDrawer(PlotView plotView)
        {
            plotModel = new PlotModel { };

            // Y-Achse im Bereich von -1 bis 1
            plotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Minimum = -1,
                Maximum = 1
            });

            waveformSeries = new LineSeries
            {
                Color = OxyColors.DarkRed,
                StrokeThickness = 1,
                MarkerSize = 0
            };
            plotModel.Series.Add(waveformSeries);
            plotView.Model = plotModel;
        }

        public void UpdateWaveform(short[] samples)
        {
            if (samples.Length == 0) return;

            // Normiere die Samples und wandle sie in Plot-Punkte um
            var plotPoints = samples
                .Select((sample, index) => new DataPoint(index, sample / 32768.0)) // Normalize samples to -1..1
                .ToArray();

            // Lösche die alten Daten und füge die neuen hinzu
            waveformSeries.Points.Clear();
            foreach (var point in plotPoints)
            {
                waveformSeries.Points.Add(point);
            }

            // Diagramm aktualisieren
            plotModel.InvalidatePlot(true);
        }
    }
}
