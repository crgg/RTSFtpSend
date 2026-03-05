namespace RtsExporter.Infrastructure;

public static class EnvLoader
{
    public static void Load(string path = ".env")
    {
        if (File.Exists(path))
        {
            DotNetEnv.Env.Load(path);
        }
    }

    public static string Get(string key, string defaultValue = "") =>
        Environment.GetEnvironmentVariable(key) ?? defaultValue;

    public static string Require(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required environment variable '{key}' is not set.");
        return value;
    }
}
