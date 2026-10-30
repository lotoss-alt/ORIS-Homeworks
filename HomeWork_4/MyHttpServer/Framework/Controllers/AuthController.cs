using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Framework.Controllers;

[Controller("auth")]
public class AuthController
{
    [Get("login")]
    public string GetLoginPage()
    {
        var viewsDir = Path.Combine(Directory.GetCurrentDirectory(), "Views");
        var htmlFile = Path.Combine(viewsDir, "index.html");

        return File.Exists(htmlFile)
            ? File.ReadAllText(htmlFile)
            : "<h1>404 - Страница входа не найдена</h1>";
    }

    [Post("login")]
    public string Login(string email, string password, bool check)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[Auth] Успешный вход пользователя:");
        Console.WriteLine($"  Email: {email}");
        Console.WriteLine($"  Password: {password}");
        Console.WriteLine($"  Запомнить меня: {check}");
        Console.ResetColor();

        var rememberStatus = check ? "да" : "нет";
        return $"<h1>Авторизация успешна</h1><p>Вы вошли как {email} (запомнить: {rememberStatus}).</p>";
    }
}
