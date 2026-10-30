using System.Text.Json;
using System.Text.Json.Serialization;
using MyHttpServer.Framework.Abstraction;

namespace MyHttpServer.Framework.Configuration;

public class Settings : AbstractSettings<Server>
{
    [JsonConstructor]
    private Settings()
    {
    }

    public override Server Server { get; set; } = new();

    public static class SingleSettings
    {
        private static readonly Lazy<Settings?> Instance =
            new(() => SetSettings(Path.Combine("Framework", "Configuration", "settings.json")));

        public static Settings? Current => Instance.Value;

        public static Settings? SetSettings(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Не найден файл конфигурации: {jsonPath}");
            }

            var json = ReadSettingsFile(jsonPath);
            return JsonSerializer.Deserialize<Settings>(json, DefaultJsonSerializerOptions);
        }
    }
}

public class Server
{
    public string Host { get; set; } = "127.0.0.1";
    public string Port { get; set; } = "8080";
    public string Path { get; set; } = "/";
}
