using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SampleToFile;

namespace Tests;

[TestClass]
public class PythonRunnerTests
{

    [TestMethod]
    public void RunPythonScript_ReturnsOutputAndErrors()
    {
        string projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;
        string scriptPath = Path.Combine(projectDir, "SampleToFile/mfcss.py");

        IPythonRunner runner = new PythonRunner();

        var (output, errors, exitCode) = runner.RunPythonScript(scriptPath);

        Assert.IsTrue(File.Exists(scriptPath), $"Script file not found: {scriptPath}");
        Assert.IsNotNull(output); 
        Assert.AreEqual(0, exitCode); 
    }
}