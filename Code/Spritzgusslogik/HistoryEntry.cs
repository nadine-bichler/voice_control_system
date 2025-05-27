using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    //Die klasse bildet Befehlseinträge
    public class HistoryEntry
    {
        public int command { get; }
        public DateTime time { get; }

        public HistoryEntry(int command)
        {
            this.command = command;
            time = DateTime.Now;
        }
    }
}
