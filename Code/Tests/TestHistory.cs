using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;

namespace Tests
{
    /// <summary>
    /// Test class for verifying the functionality of the <c>History</c> and <c>HistoryEntry</c> classes.
    /// </summary>
    [TestClass]
    public class TestHistory
    {
        /// <summary>
        /// Tests whether a single <c>HistoryEntry</c> is added correctly to the history.
        /// </summary>
        [TestMethod]
        public void Test_AddEntry()
        {
            var history = new History();
            int expectedCommand = 3;

            history.AddEntry(expectedCommand);

            Assert.AreEqual(1, history.entries.Count, "Entry count is incorrect.");
            Assert.AreEqual(expectedCommand, history.entries[0].command, "Stored command is incorrect.");
            Assert.IsTrue((DateTime.Now - history.entries[0].time).TotalSeconds < 2, "Timestamp is not recent.");
        }

        /// <summary>
        /// Tests whether multiple <c>HistoryEntry</c> objects are added correctly in sequence.
        /// </summary>
        [TestMethod]
        public void TestMany_AddEntry()
        {
            var history = new History();
            int[] commands = { 3, 7, 9 };

            foreach (var cmd in commands)
            {
                history.AddEntry(cmd);
            }

            Assert.AreEqual(3, history.entries.Count, "Entry count is incorrect.");
            for (int i = 0; i < history.entries.Count; i++)
            {
                Assert.AreEqual(commands[i], history.entries[i].command, $"Stored command at index {i} is incorrect.");
                Assert.IsTrue((DateTime.Now - history.entries[i].time).TotalSeconds < 2, $"Timestamp at index {i} is not recent.");
            }
        }

        /// <summary>
        /// Tests that a newly initialized history is empty.
        /// </summary>
        [TestMethod]
        public void TestEmpty_History()
        {
            var history = new History();

            Assert.AreEqual(0, history.entries.Count, "History should be empty.");
        }

        /// <summary>
        /// Tests that invalid command value "0" throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [TestMethod]
        public void TestInvalidCommand_Zero()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new HistoryEntry(0));
        }

        /// <summary>
        /// Tests that invalid command value "10" throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [TestMethod]
        public void TestInvalidCommand_Ten()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new HistoryEntry(10));
        }

        /// <summary>
        /// Tests that invalid command value (negative) throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [TestMethod]
        public void TestInvalidCommand_Negative()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new HistoryEntry(-1));
        }
    }
}
