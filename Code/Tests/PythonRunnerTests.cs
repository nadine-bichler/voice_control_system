using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SampleToFile;

namespace Tests;

/// <summary>
/// Tests Python Code for sample files
/// </summary>
[TestClass]
public class PythonRunnerTests
{
    /// <summary>
    /// tests if files are being created correctly
    /// </summary>
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