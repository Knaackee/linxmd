using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Linxmd.Services;

public static class ClipboardService
{
    public static bool TryCopy(string text, out string? error)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return TryCopyWithProcess("cmd.exe", "/c clip", text, out error);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return TryCopyWithProcess("pbcopy", "", text, out error);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (TryCopyWithProcess("wl-copy", "", text, out _))
            {
                error = null;
                return true;
            }

            if (TryCopyWithProcess("xclip", "-selection clipboard", text, out _))
            {
                error = null;
                return true;
            }

            if (TryCopyWithProcess("xsel", "--clipboard --input", text, out _))
            {
                error = null;
                return true;
            }

            error = "No clipboard command available (tried wl-copy, xclip, xsel).";
            return false;
        }

        error = "Unsupported OS for clipboard copy.";
        return false;
    }

    private static bool TryCopyWithProcess(string fileName, string arguments, string input, out string? error)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                error = $"Could not start process '{fileName}'.";
                return false;
            }

            process.StandardInput.Write(input);
            process.StandardInput.Close();
            process.WaitForExit(5000);

            if (process.ExitCode == 0)
            {
                error = null;
                return true;
            }

            var stderr = process.StandardError.ReadToEnd();
            error = string.IsNullOrWhiteSpace(stderr)
                ? $"Clipboard process '{fileName}' exited with code {process.ExitCode}."
                : stderr.Trim();
            return false;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
