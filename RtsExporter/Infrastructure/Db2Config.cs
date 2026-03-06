namespace RtsExporter.Infrastructure;

public static class Db2Config
{
    public static string ConnectionString
    {
        get
        {
            var dsn = EnvLoader.Get("DSN") ?? EnvLoader.Get("DB_DSN");
            if (string.IsNullOrWhiteSpace(dsn))
                throw new InvalidOperationException("DSN is required. Set DSN in .env (e.g. DSN=RG3).");

            // Por defecto: DSN sin credenciales (cada servidor tiene su DSN configurado)
            var useCreds = EnvLoader.Get("DSN_USE_CREDENTIALS");
            if (string.IsNullOrWhiteSpace(useCreds) || (useCreds != "1" && !useCreds.Equals("true", StringComparison.OrdinalIgnoreCase)))
                return $"DSN={dsn};";

            var user = EnvLoader.Require("DB_USER");
            var password = EnvLoader.Require("DB_PASSWORD");
            return $"DSN={dsn};UID={user};PWD={password};";
        }
    }
}
