using System.Diagnostics;

namespace SampleToFile;

public interface IPythonRunner
{
    (string output, string errors, int exitCode) RunPythonScript(string scriptPath);
}

public class PythonRunner : IPythonRunner
{
    public (string output, string errors, int exitCode) RunPythonScript(string scriptPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        string output = process.StandardOutput.ReadToEnd();
        string errors = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return (output, errors, process.ExitCode);
    }
}