using System.Text.Json;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.Results;
using MyHttpServer.Framework.Services;

namespace MyHttpServer.Framework.Controllers;

[Controller("Spam")]
public class DdosController : ControllerBase
{
    private readonly EmailService _emailService;

    public DdosController()
    {
        var settingsPath = Path.Combine("Framework", "Configuration", "Email", "EmailSettings.json");
        var json = File.ReadAllText(settingsPath);
        var emailSettings = JsonSerializer.Deserialize<EmailSettings>(json)
                            ?? throw new InvalidOperationException("Не удалось загрузить EmailSettings.");
        _emailService = new EmailService(emailSettings);
    }

    [Get("Start")]
    public IActionResult Send(string toEmail, int count)
    {
        if (count > 100)
        {
            return Html("<h1>Ошибка</h1><p>Превышен допустимый лимит сообщений за один запрос (максимум 100).</p>");
        }

        var attachmentPath = Path.Combine("attachment.mp3");
        var hasAttachment = File.Exists(attachmentPath);

        Task.Run(async () =>
        {
            for (var i = 0; i < count; i++)
            {
                try
                {
                    _emailService.SendEmail(
                        toEmail,
                        $"[Уведомление {i + 1} из {count}]",
                        "Тестовая рассылка уведомлений от HTTP-сервера MyHttpServer.",
                        isHtml: false,
                        hasAttachment ? attachmentPath : null);

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mail Error #{i + 1}]: {ex.Message}");
                }
            }
        });

        return Html($"<h1>Рассылка запущена</h1><p>Отправка на адрес {toEmail} ({count} писем).</p>");
    }
}
