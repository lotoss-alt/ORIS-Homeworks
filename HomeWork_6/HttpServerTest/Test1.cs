using MiniTemplateEngine;

namespace HttpServerTest;

[TestClass]
public sealed class Test1
{
    private readonly HtmlTemplateRenderer _renderer = new();

    [TestMethod]
    public void RenderFromString_ReplacesSimpleProperties()
    {
        var template = "<h1>Привет, ${Name}!</h1><p>Курс: ${Course}</p>";
        var model = new { Name = "Студент", Course = 2 };

        var actual = _renderer.RenderFromString(template, model);

        Assert.AreEqual("<h1>Привет, Студент!</h1><p>Курс: 2</p>", actual);
    }

    [TestMethod]
    public void RenderFromString_EvaluatesIfElseBlock()
    {
        var template = "$if(IsAuthorized)<p>Добро пожаловать, ${User}</p>$else<p>Доступ запрещен</p>$endif";

        var authorizedHtml = _renderer.RenderFromString(template, new { IsAuthorized = true, User = "Admin" });
        var guestHtml = _renderer.RenderFromString(template, new { IsAuthorized = false, User = "Admin" });

        Assert.AreEqual("<p>Добро пожаловать, Admin</p>", authorizedHtml);
        Assert.AreEqual("<p>Доступ запрещен</p>", guestHtml);
    }

    [TestMethod]
    public void RenderFromString_RendersForeachCollection()
    {
        var template = "<ul>$foreach(var item in Items)<li>${item}</li>$endfor</ul>";
        var model = new { Items = new[] { "C#", "HTTP", "Templates" } };

        var actual = _renderer.RenderFromString(template, model);

        Assert.AreEqual("<ul><li>C#</li><li>HTTP</li><li>Templates</li></ul>", actual);
    }
}
