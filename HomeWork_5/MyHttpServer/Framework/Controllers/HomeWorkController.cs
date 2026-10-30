using System.IO.Compression;
using System.Text.Json;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using MyHttpServer.Framework.Services;

namespace MyHttpServer.Framework.Controllers;

[Controller("HomeWork")]
public class HomeWorkController : ControllerBase
{
    [Get("Send")]
    public IActionResult Send(string toEmail)
    {
        var settingsPath = Path.Combine("Framework", "Configuration", "Email", "EmailSettings.json");
        var json = File.ReadAllText(settingsPath);
        var emailSettings = JsonSerializer.Deserialize<EmailSettings>(json)
                            ?? throw new InvalidOperationException("Не удалось загрузить EmailSettings.");

        var emailService = new EmailService(emailSettings);
        var zipPath = Path.Combine(Path.GetTempPath(), "Framework.zip");

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        var frameworkDir = Path.Combine(Directory.GetCurrentDirectory(), "Framework");
        ZipFile.CreateFromDirectory(frameworkDir, zipPath);

        emailService.SendEmail(
            toEmail,
            "Домашнее задание по ОРИС",
            "Отправка архива с исходным кодом фреймворка MyHttpServer.\nРепозиторий: https://github.com/lotoss-alt/ORIS-Homeworks",
            isHtml: false,
            attachmentFile: zipPath);

        return Html("<h1>Готово!</h1><p>Архив с фреймворком успешно отправлен на указанную почту.</p>");
    }
}
