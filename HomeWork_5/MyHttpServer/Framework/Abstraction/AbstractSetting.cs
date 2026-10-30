using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyHttpServer.Framework.Abstraction;

public abstract class AbstractSettings<TServer>
{
    public abstract TServer Server { get; set; }

    protected static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [JsonConstructor]
    protected AbstractSettings()
    {
    }

    protected static string ReadSettingsFile(string path) => File.ReadAllText(path);

    protected static T? BaseSetSettings<T>(string jsonPath) where T : AbstractSettings<TServer>
    {
        var json = ReadSettingsFile(jsonPath);
        return JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
    }
}
