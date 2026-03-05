using System.Linq;
using RtsExporter.Infrastructure;
using RtsExporter.Services;
using Serilog;

namespace RtsExporter;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Load .env: current dir, parent, or next to executable
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env")),
            ".env"
        };
        var envPath = candidates.FirstOrDefault(File.Exists) ?? ".env";
        EnvLoader.Load(envPath);
        Logger.Configure("logs/app.log");

        try
        {
            Log.Information("Starting export process");

            var registerService = new RegisterService();
            var (lastStart, currStart) = await registerService.GetWindowAsync();

            var db2Service = new Db2Service();
            var records = await db2Service.ExecuteQueryAsync(lastStart, currStart);

            if (records.Count == 0)
            {
                Log.Warning("No records to export");
                await registerService.UpdateAfterSuccessAsync(currStart);
                return 0;
            }

            var csvService = new CsvService();
            var csvPath = await csvService.GenerateCsvAsync(records);

            Log.Information("Uploading file to FTP...");
            var ftpService = new FtpService();
            await ftpService.UploadAsync(csvPath);

            await registerService.UpdateAfterSuccessAsync(currStart);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Export failed");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
