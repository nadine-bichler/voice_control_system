using Spritzgusslogik;

namespace Tests;

[TestClass]
public class TestHistory
{
    [TestMethod]
    public void TestAddEntry()
    {
        var history = new History();
        int expectedCommand = 3;

        history.AddEntry(expectedCommand);

        Assert.AreEqual(1, history.entries.Count, "Anazhl der Einträge stimmt nicht");
        Assert.AreEqual(expectedCommand, history.entries[0].command, "gespeicherter Befehl stimmt nicht");
        Assert.IsTrue((DateTime.Now - history.entries[0].time).TotalSeconds < 2, "Nicht aktueller Zeitstempel");
    }
}
