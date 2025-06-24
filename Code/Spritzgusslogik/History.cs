///
/// @file    History
/// @brief   Command History
/// @author  Bichler Nadine
/// @date    24.06.2025
///
///

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spritzgusslogik
{
    /// <summary>
    /// creats a list with HistoryEntries
    /// </summary>
    public class History
    {
        public readonly List<HistoryEntry> entries = new();

        /// <summary>
        /// Creates a new HistoryEntry and adds it into the list
        /// </summary>
        /// <param name="command">digit command</param>
        /// <returns>new history entry</returns>
        public HistoryEntry AddEntry(int command)
        {
            var entry = new HistoryEntry(command);
            entries.Add(entry);
            return entry;
        }
    }
}
