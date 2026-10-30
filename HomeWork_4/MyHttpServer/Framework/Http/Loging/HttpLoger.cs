using System.Net;

namespace MyHttpServer.Framework.Http.Loging;

public static class HttpLoger
{
    public static void LogRequestInfo(HttpListenerContext context)
    {
        var request = context.Request;
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var method = request.HttpMethod;
        var rawUrl = request.RawUrl ?? "/";
        var clientIp = request.RemoteEndPoint?.ToString() ?? "Unknown";
        var userAgent = request.UserAgent ?? "Unknown";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[{timestamp}] {method} {rawUrl}");
        Console.ResetColor();
        Console.WriteLine($"  Клиент: {clientIp}");
        Console.WriteLine($"  User-Agent: {userAgent}");
        Console.WriteLine(new string('-', 48));
    }
}
