namespace RtsExporter.Infrastructure;

public static class Db2Config
{
    private const string DefaultHost = "10.100.1.2";
    private const string DefaultDatabase = "GOTOTEST";
    private const string DefaultUser = "TMWIN";
    private const string DefaultPassword = "goto2525";
    private const int DefaultPort = 50000;

    public static string ConnectionString
    {
        get
        {
            var host = GetEnvOrDefault("DB2_HOST", "DB_HOST", DefaultHost);
            var database = GetEnvOrDefault("DB2_DB", "DB_NAME", DefaultDatabase);
            var user = GetEnvOrDefault("DB2_USER", "DB_USER", DefaultUser);
            var password = GetEnvOrDefault("DB2_PASS", "DB_PASSWORD", DefaultPassword);
            var portValue = GetEnvOrDefault("DB2_PORT", "DB_PORT", DefaultPort.ToString());
            if (!int.TryParse(portValue, out var port))
                port = DefaultPort;

            return $"Server={host}:{port};Database={database};UID={user};PWD={password};";
        }
    }

    private static string GetEnvOrDefault(string primary, string fallback, string defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(primary)
            ?? Environment.GetEnvironmentVariable(fallback);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
