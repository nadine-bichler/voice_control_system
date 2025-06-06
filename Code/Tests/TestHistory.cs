using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Spritzgusslogik;


namespace Tests;

[TestClass]
public class TestHistory
{
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

    [TestMethod]
    public void TestEmpty_History()
    {
        var history = new History();

        Assert.AreEqual(0, history.entries.Count, "Es sollten keine Einträge vorhanden sein");
    }
}
