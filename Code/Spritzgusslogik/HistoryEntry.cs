///
/// @file    HistoryEntry
/// @brief   History Entries for History
/// @author  Bichler Nadine
/// @date    24.06.2025
///
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    /// <summary>
    /// history entry with a command and a timestamp
    /// </summary>
    public class HistoryEntry
    {
        public int command { get; }
        public DateTime time { get; }

        /// <summary>
        /// creates a new HistoryEntry
        /// </summary>
        /// <param name="command">digit command</param>
        public HistoryEntry(int command)
        {
            this.command = command;
            time = DateTime.Now;
        }
    }
}
