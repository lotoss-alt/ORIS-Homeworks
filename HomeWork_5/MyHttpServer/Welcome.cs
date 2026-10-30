using MyHttpServer.Framework.Configuration;
using MyHttpServer.Framework.Handlers;
using MyHttpServer.Framework.Http;
using MyHttpServer.Framework.Routing;

namespace MyHttpServer;

public class Welcome
{
    public async Task Run()
    {
        while (true)
        {
            Console.WriteLine("Введите 'start' для запуска сервера или 'exit' для выхода:");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (input == "exit")
            {
                break;
            }

            if (input == "start")
            {
                var currentDir = Directory.GetCurrentDirectory();
                var staticDir = Path.Combine(currentDir, "static");
                Console.WriteLine($"[Config] Папка статики: {staticDir}");

                var settings = Settings.SingleSettings.Current
                               ?? throw new InvalidOperationException("Не удалось загрузить настройки сервера.");
                var mimeResolver = new MimeTypeResolver();

                var rootHandler = new ExceptionHandler();
                var staticFilesHandler = new StaticFilesHandler(staticDir, mimeResolver);
                var controllersHandler = new ControllersHandler();
                var notFoundHandler = new NotFoundHandler(staticDir);

                rootHandler
                    .SetNext(staticFilesHandler)
                    .SetNext(controllersHandler)
                    .SetNext(notFoundHandler);

                var httpServer = new HttpServer(settings, rootHandler);
                var host = new ServerHost(httpServer);
                await host.StartHostAsync();
            }
        }
    }
}
