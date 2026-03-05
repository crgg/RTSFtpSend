#pragma warning disable SYSLIB0014 // FTP requires FtpWebRequest
using System.Net;
using RtsExporter.Infrastructure;
using Serilog;

namespace RtsExporter.Services;

public class FtpService
{
    private readonly string _host;
    private readonly string _user;
    private readonly string _password;

    public FtpService()
    {
        _host = EnvLoader.Require("FTP_HOST");
        _user = EnvLoader.Require("FTP_USER");
        _password = EnvLoader.Require("FTP_PASSWORD");
    }

    public async Task UploadAsync(string localFilePath, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(localFilePath);
        var uri = new Uri($"ftp://{_host}/{fileName}");

        var request = (FtpWebRequest)WebRequest.Create(uri);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = new NetworkCredential(_user, _password);
        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = false;

        var fileContent = await File.ReadAllBytesAsync(localFilePath, ct);

        request.ContentLength = fileContent.Length;

        await using var requestStream = await request.GetRequestStreamAsync();
        await requestStream.WriteAsync(fileContent, ct);

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        if (response.StatusCode >= FtpStatusCode.ClosingData && response.StatusCode < FtpStatusCode.ClosingData + 100)
        {
            Log.Information("Upload completed successfully");
        }
        else
        {
            throw new InvalidOperationException($"FTP upload failed: {response.StatusDescription}");
        }
    }
}
