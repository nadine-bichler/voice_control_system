///
/// @file    HistoryEntry
/// @brief   History Entries for History
/// @author  Bichler Nadine
/// @date    24.06.2025
///
///


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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public HistoryEntry(int command)
        {
            if (command < 1 || command > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(command), "Command must be a digit between 1 and 9.");
            }
            this.command = command;
            time = DateTime.Now;
        }
    }
}
