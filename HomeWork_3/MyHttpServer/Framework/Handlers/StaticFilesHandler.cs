using System.Net;
using MyHttpServer.Framework.Routing;

namespace MyHttpServer.Framework.Handlers;

public class StaticFilesHandler(string staticFolder, MimeTypeResolver mimeTypeResolver) : BaseHandler
{
    public override async Task HandleAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;
            var relativePath = request.Url!.LocalPath;
            var fullPath = Path.GetFullPath(Path.Combine(staticFolder, relativePath.TrimStart('/')));

            if (!fullPath.StartsWith(staticFolder + Path.DirectorySeparatorChar) && fullPath != staticFolder)
            {
                response.StatusCode = 403;
                fullPath = Path.Combine(staticFolder, "403.html");
            }
            else if (Directory.Exists(fullPath))
            {
                if (!relativePath.EndsWith('/'))
                {
                    response.StatusCode = 301;
                    response.Headers.Add("Location", relativePath + "/");
                    response.Close();
                    return;
                }

                fullPath = Path.Combine(fullPath, "index.html");
            }

            if (response.StatusCode == 200 && !File.Exists(fullPath))
            {
                await base.HandleAsync(context);
                context.Response.Close();
                return;
            }

            var extension = Path.GetExtension(fullPath);
            response.ContentType = mimeTypeResolver.GetType(extension);

            if (File.Exists(fullPath))
            {
                await SendResponseAsync(fullPath, response);
                response.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[StaticFilesHandler] Ошибка: {ex.Message}");
            context.Response.StatusCode = 500;
        }
    }
}
