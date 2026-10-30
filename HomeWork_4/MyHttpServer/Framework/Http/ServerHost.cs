namespace MyHttpServer.Framework.Http;

public class ServerHost(HttpServer server)
{
    public async Task StartHostAsync()
    {
        using var cts = new CancellationTokenSource();
        var serverTask = Task.Run(() => server.StartAsync(cts.Token));

        Console.WriteLine("[ServerHost] Сервер запущен. Введите 'exit' для остановки.");
        while (true)
        {
            var command = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (command == "exit")
            {
                Console.WriteLine("[ServerHost] Остановка сервера...");
                await cts.CancelAsync();
                break;
            }

            Console.WriteLine("Для остановки сервера введите 'exit'.");
        }

        await serverTask;
        Console.WriteLine("[ServerHost] Сервер успешно остановлен.");
    }
}
