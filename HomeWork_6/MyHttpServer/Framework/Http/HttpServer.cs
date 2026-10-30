using System.Net;
using MyHttpServer.Framework.Abstraction;
using MyHttpServer.Framework.Configuration;
using MyHttpServer.Framework.Handlers;
using MyHttpServer.Framework.Http.Loging;

namespace MyHttpServer.Framework.Http;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly AbstractSettings<Server> _settings;
    private readonly BaseHandler _handler;

    public HttpServer(AbstractSettings<Server> settings, BaseHandler handler)
    {
        _settings = settings;
        _handler = handler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var prefix = $"http://{_settings.Server.Host}:{_settings.Server.Port}{_settings.Server.Path}";
            Console.WriteLine($"[HttpServer] Прослушивание по адресу: {prefix}");
            _listener.Prefixes.Add(prefix);

            _listener.Start();
            await using var registration = cancellationToken.Register(() => _listener.Stop());

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    HttpLoger.LogRequestInfo(context);
                    _ = Task.Run(() => _handler.HandleAsync(context), cancellationToken);
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HttpServer] Ошибка обработки запроса: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HttpServer] Критическая ошибка: {ex.Message}");
            throw;
        }
        finally
        {
            _listener.Close();
        }
    }
}
