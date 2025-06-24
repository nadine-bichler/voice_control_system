///
/// @file    History.cs
/// @brief   Maintains a list of digit command history entries with timestamps.
/// @author  Bichler Nadine
/// @date    24.06.2025
///

namespace Spritzgusslogik
{
    /// <summary>
    /// Manages a list of <see cref="HistoryEntry"/> objects representing command history.
    /// </summary>
    public class History
    {
        /// <summary>
        /// A list containing all recorded history entries.
        /// </summary>
        public readonly List<HistoryEntry> entries = new();

        /// <summary>
        /// Creates a new <see cref="HistoryEntry"/> for the specified digit command and adds it to the history.
        /// </summary>
        /// <param name="command">A digit command between 1 and 9.</param>
        /// <returns>The newly created <see cref="HistoryEntry"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the command is not within the valid range.</exception>
        public HistoryEntry AddEntry(int command)
        {
            var entry = new HistoryEntry(command);
            entries.Add(entry);
            return entry;
        }
    }
}