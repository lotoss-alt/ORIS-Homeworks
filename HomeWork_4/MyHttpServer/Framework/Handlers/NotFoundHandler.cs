using MyHttpServer.Framework.Handlers;
using System.Net;

namespace MyHttpServer.Framework.Handlers;

public class NotFoundHandler(string staticFolder) : BaseHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        var response = context.Response;
        response.StatusCode = 404;

        var notFoundPage = Path.Combine(staticFolder, "404.html");
        if (File.Exists(notFoundPage))
        {
            await SendResponseAsync(notFoundPage, response);
        }
        else
        {
            await SendTextAsync("<h1>404 — Страница не найдена</h1>", response);
        }

        response.Close();
    }
}
