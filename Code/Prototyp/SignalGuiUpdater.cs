using Spritzgusslogik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Prototyp
{
    // Diese Klasse ruft alle 30ms die Mikrofonsignale ab und ruft den SignalDrawer auf, um sie zu zeichnen
    class SignalGuiUpdater
    {

        private DispatcherTimer timer;

        private Microphone microphone;
        private SignalDrawer signalDrawer;

        public SignalGuiUpdater(Microphone microphone, SignalDrawer signalDrawer)
        {
            this.microphone = microphone;
            this.signalDrawer = signalDrawer;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (microphone.IsMuted())
            {
                return;
            }

            var values = microphone.GetValues();
            if (values != null)
                signalDrawer.UpdateWaveform(values);
        }
    }
}
