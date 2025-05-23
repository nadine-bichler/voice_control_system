using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    internal class HistoryEntry
    {
        private int command;
        private string time => DateTime.Now.ToString("HH:mm:ss");
    }
}
