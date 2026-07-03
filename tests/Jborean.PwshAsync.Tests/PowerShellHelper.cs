using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jborean.PwshAsync.Tests;

public static class PowerShellHelper
{
    /// <summary>
    /// Invokes a PowerShell cmdlet by importing a module and running a command.
    /// Validates that the command succeeds (exit code 0) and returns stdout.
    /// If the command fails, throws an exception with stdout/stderr for debugging.
    /// </summary>
    /// <param name="modulePath">Path to the .dll assembly to import</param>
    /// <param name="cmdlet">The cmdlet to run (e.g., "Test-AsyncCmdlet")</param>
    /// <param name="parameters">Optional parameters to pass to the cmdlet (e.g., "-Name Value")</param>
    /// <returns>The stdout from the successful command execution</returns>
    /// <exception cref="InvalidOperationException">Thrown when the command fails (non-zero exit code)</exception>
    public static async Task<string> InvokeCmdletAsync(
        string modulePath,
        string cmdlet,
        string? parameters = null)
    {
        string command = $"Import-Module -Name '{modulePath}'; {cmdlet}";
        if (!string.IsNullOrEmpty(parameters))
        {
            command = $"Import-Module -Name '{modulePath}'; {cmdlet} {parameters}";
        }

        return await RunPowerShellAsync(command);
    }

    /// <summary>
    /// Runs a PowerShell command directly.
    /// </summary>
    /// <param name="command">The PowerShell command to execute</param>
    /// <returns>The stdout from the successful command execution</returns>
    public static async Task<string> RunPowerShellAsync(string command)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "pwsh",
            Arguments = $"-NoProfile -Command \"{command.Replace("\"", "\\\"")}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process? process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start pwsh process");
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        await Task.WhenAll(stdoutTask, stderrTask);
        await process.WaitForExitAsync();

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            StringBuilder errorMessage = new StringBuilder();
            errorMessage.AppendLine($"PowerShell command failed with exit code {process.ExitCode}");
            errorMessage.AppendLine($"Command: {command}");
            errorMessage.AppendLine();
            errorMessage.AppendLine("=== STDOUT ===");
            errorMessage.AppendLine(string.IsNullOrEmpty(stdout) ? "(empty)" : stdout.TrimEnd());
            errorMessage.AppendLine();
            errorMessage.AppendLine("=== STDERR ===");
            errorMessage.AppendLine(string.IsNullOrEmpty(stderr) ? "(empty)" : stderr.TrimEnd());

            throw new InvalidOperationException(errorMessage.ToString());
        }

        return stdout;
    }

    /// <summary>
    /// Gets the path to a test project's output assembly.
    /// </summary>
    /// <param name="projectName">The name of the test project (e.g., "SampleCmdlet")</param>
    /// <returns>Full path to the built assembly</returns>
    public static string GetTestProjectPath(string projectName)
    {
        string baseDir = AppContext.BaseDirectory;
        string projectDir = Path.Combine(baseDir, "..", "..", "..", "..", projectName, "bin", "Debug", "net9.0");
        string assemblyPath = Path.Combine(projectDir, $"{projectName}.dll");

        return Path.GetFullPath(assemblyPath);
    }
}
