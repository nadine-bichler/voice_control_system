using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;


namespace Tests;

/// <summary>
/// This class tests the History
/// </summary>
[TestClass]
public class TestHistory
{
    /// <summary>
    /// tests adding one historyEntry
    /// </summary>
    [TestMethod]
    public void Test_AddEntry()
    {
        var history = new History();
        int expectedCommand = 3;

        history.AddEntry(expectedCommand);

        Assert.AreEqual(1, history.entries.Count, "Anazhl der Einträge stimmt nicht");
        Assert.AreEqual(expectedCommand, history.entries[0].command, "Gespeicherter Befehl stimmt nicht");
        Assert.IsTrue((DateTime.Now - history.entries[0].time).TotalSeconds < 2, "Nicht aktueller Zeitstempel");
    }

    /// <summary>
    /// tests adding multiple history entries
    /// </summary>
    [TestMethod]
    public void TestMany_AddEntry()
    {
        var history = new History();
        int[] commands = {3, 7, 9};

        foreach (var cmd in commands)
        {
            history.AddEntry(cmd);
        }

        Assert.AreEqual(3, history.entries.Count, "Anazhl der Einträge stimmt nicht");
        for(int i = 0; i < history.entries.Count; i++)
        {
            Assert.AreEqual(commands[i], history.entries[i].command, $"Gespeicherter Befehl an Stelle {i} stimmt nicht");
            Assert.IsTrue((DateTime.Now - history.entries[i].time).TotalSeconds < 2, $"Zeitstempel an Stelle {i} stimmt nicht");
        }
    }

    /// <summary>
    /// tests if new history is empty
    /// </summary>
    [TestMethod]
    public void TestEmpty_History()
    {
        var history = new History();

        Assert.AreEqual(0, history.entries.Count, "Es sollten keine Einträge vorhanden sein");
    }

    /// <summary>
    /// tests if invalid commands throw exceptions
    /// </summary>
    [TestMethod]
    public void TestInvalidCommand_History()
    {
        // Test: Ungültiger Befehl (0)
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            var entry = new HistoryEntry(0);
        });

        // Test: Ungültiger Befehl (10)
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            var entry = new HistoryEntry(10);
        });

        // Test: Ungültiger Befehl (-1)
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        {
            var entry = new HistoryEntry(-1);
        });
    }
}
