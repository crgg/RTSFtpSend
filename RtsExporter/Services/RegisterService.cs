using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

#pragma warning disable CA1416 // Registry only used when IsWindows
using RtsExporter.Infrastructure;
using Serilog;

namespace RtsExporter.Services;

public class RegisterService
{
    private readonly string _registryPath;
    private readonly string _fallbackLastStart;
    private readonly string _fileFallbackPath;
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public RegisterService()
    {
        _registryPath = EnvLoader.Get("REGISTRY_PATH", @"Software\RtsExporter");
        _fallbackLastStart = EnvLoader.Get("LASTSTART_FALLBACK", "");
        _fileFallbackPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            EnvLoader.Get("REGISTER_FILE", ".rts_laststart"));
    }

    /// <summary>
    /// Reads LASTSTART from Windows Registry (or file on non-Windows). CURRSTART is always current time.
    /// </summary>
    public Task<(string LastStart, string CurrStart)> GetWindowAsync(CancellationToken ct = default)
    {
        var currStart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var lastStart = IsWindows ? ReadFromRegistry() : ReadFromFile();

        if (string.IsNullOrWhiteSpace(lastStart))
        {
            if (string.IsNullOrWhiteSpace(_fallbackLastStart))
                throw new InvalidOperationException(
                    "No LASTSTART in registry. Set LASTSTART_FALLBACK in .env for first run.");

            Log.Information("Using LASTSTART_FALLBACK for first run: {LastStart}", _fallbackLastStart);
            return Task.FromResult((_fallbackLastStart, currStart));
        }

        Log.Information("Register: LASTSTART={LastStart}, CURRSTART={CurrStart}", lastStart, currStart);
        return Task.FromResult((lastStart, currStart));
    }

    /// <summary>
    /// Updates the register with CURRSTART as the new LASTSTART for next run.
    /// </summary>
    public Task UpdateAfterSuccessAsync(string currStart, CancellationToken ct = default)
    {
        if (IsWindows)
            WriteToRegistry(currStart);
        else
            WriteToFile(currStart);

        Log.Information("Register updated: LASTSTART={LastStart} for next run", currStart);
        return Task.CompletedTask;
    }

    [SupportedOSPlatform("windows")]
    private string? ReadFromRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(_registryPath);
            return key?.GetValue("LASTSTART")?.ToString()?.Trim();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not read from registry, using fallback");
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    private void WriteToRegistry(string value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(_registryPath);
            key?.SetValue("LASTSTART", value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to write to registry");
            throw;
        }
    }

    private string? ReadFromFile()
    {
        try
        {
            if (!File.Exists(_fileFallbackPath))
                return null;
            return File.ReadAllText(_fileFallbackPath).Trim();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not read register file");
            return null;
        }
    }

    private void WriteToFile(string value)
    {
        try
        {
            File.WriteAllText(_fileFallbackPath, value);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to write register file");
            throw;
        }
    }
}
