///
/// @file    HistoryEntry.cs
/// @brief   Defines entries used to store recognized digit commands with timestamps.
/// @author  Bichler Nadine
/// @date    24.06.2025
///

namespace Spritzgusslogik
{
    /// <summary>
    /// Represents a single history entry consisting of a recognized digit command and the corresponding timestamp.
    /// </summary>
    public class HistoryEntry
    {
        /// <summary>
        /// Gets the recognized digit command (between 1 and 9).
        /// </summary>
        public int command { get; }

        /// <summary>
        /// Gets the timestamp when the command was created.
        /// </summary>
        public DateTime time { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryEntry"/> class.
        /// </summary>
        /// <param name="command">A digit command between 1 and 9.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the command is not between 1 and 9.</exception>
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