using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    internal class HistoryEntry
    {
        private int command { get; }
        public DateTime time { get; }

        public HistoryEntry(int command)
        {
            this.command = command;
            time = DateTime.Now;
        }
    }
}
