using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RtsExporter.Infrastructure;
using RtsExporter.Models;
using Serilog;

namespace RtsExporter.Services;

public class CsvService
{
    private readonly string _outputFolder;

    public CsvService()
    {
        var pathCsv = EnvLoader.Get("PATH_CSVFILE");
        _outputFolder = !string.IsNullOrWhiteSpace(pathCsv) 
            ? pathCsv 
            : EnvLoader.Get("OUTPUT_FOLDER", "./output");
    }

    public async Task<string> GenerateCsvAsync(IReadOnlyList<InvoiceRecord> records)
    {
        Directory.CreateDirectory(_outputFolder);

        var now = DateTime.Now;
        var fileName = $"RTS_{now:yyyyMMddHHmmssfff}.csv";
        var filePath = Path.Combine(_outputFolder, fileName);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        };

        await using var stream = File.Create(filePath);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        using var csv = new CsvWriter(writer, config);

        await csv.WriteRecordsAsync(records);

        Log.Information("CSV generated: {FilePath}", filePath);
        return filePath;
    }
}
