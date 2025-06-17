using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    //Die Klasse enthält die Liste der Befehlseinträge
    public class History
    {
        public readonly List<HistoryEntry> entries = new();

        public HistoryEntry AddEntry(int command)
        {
            var entry = new HistoryEntry(command);
            entries.Add(entry);
            return entry;
        }
    }
}
