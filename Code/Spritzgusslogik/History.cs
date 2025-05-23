using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    //Die Klasse enthält die Liste der Befehlseinträge
    internal class History
    {
        private List<HistoryEntry> entries = new();

        public void AddEntry(int command)
        {
            entries.Add(new HistoryEntry(command));
        }

        public List<HistoryEntry> SortEntries()
        {
            return entries.OrderBy(e => e.time).ToList();
        }
    }
}
