using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniTemplateEngine;

/// <summary>
/// Реализация HTML-шаблонизатора с поддержкой подстановки переменных,
/// условных блоков ($if / $else / $endif) и циклов ($foreach / $endfor).
/// </summary>
public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    public record TemplateToken(TokenType Type, string Content);

    private static readonly Regex DirectiveRegex = new(
        @"(\$if\s*\(.*?\)|\$else\b|\$endif\b|\$foreach\s*\(.*?\)|\$endfor\b)",
        RegexOptions.Compiled);

    private static readonly Regex InterpolationRegex = new(
        @"[$]\{([^}]+)\}",
        RegexOptions.Compiled);

    private static readonly Regex ForeachHeaderRegex = new(
        @"[$]foreach\s*\(\s*(?:var\s+)?([a-zA-Z0-9_]+)\s+in\s+([a-zA-Z0-9_.]+)\s*\)",
        RegexOptions.Compiled);

    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        ArgumentNullException.ThrowIfNull(htmlTemplate);
        ArgumentNullException.ThrowIfNull(dataModel);

        var tokens = Tokenize(htmlTemplate);
        var currentIndex = 0;
        var scopeVariables = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        return ProcessTokens(tokens, ref currentIndex, dataModel, scopeVariables);
    }

    public string RenderFromFile(string filePath, object dataModel)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(dataModel);

        var templateContent = File.ReadAllText(filePath, Encoding.UTF8);
        return RenderFromString(templateContent, dataModel);
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        ArgumentNullException.ThrowIfNull(inputFilePath);
        ArgumentNullException.ThrowIfNull(outputFilePath);
        ArgumentNullException.ThrowIfNull(dataModel);

        var renderedHtml = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, renderedHtml, Encoding.UTF8);
        return renderedHtml;
    }

    private static List<TemplateToken> Tokenize(string template)
    {
        var result = new List<TemplateToken>();
        var segments = DirectiveRegex.Split(template);

        foreach (var segment in segments)
        {
            if (string.IsNullOrEmpty(segment))
                continue;

            var tokenType = segment switch
            {
                _ when segment.StartsWith("$if") => TokenType.If,
                "$else" => TokenType.Else,
                "$endif" => TokenType.EndIf,
                _ when segment.StartsWith("$foreach") => TokenType.Foreach,
                "$endfor" => TokenType.EndFor,
                _ => TokenType.Text
            };

            result.Add(new TemplateToken(tokenType, segment));
        }

        return result;
    }

    private static string ParseIfCondition(string ifDirective)
    {
        var openParen = ifDirective.IndexOf('(');
        var closeParen = ifDirective.LastIndexOf(')');

        if (openParen != -1 && closeParen > openParen)
        {
            return ifDirective.Substring(openParen + 1, closeParen - openParen - 1).Trim();
        }

        return string.Empty;
    }

    private static (string iteratorName, string sourcePath) ParseForeachHeader(string foreachDirective)
    {
        var match = ForeachHeaderRegex.Match(foreachDirective);
        if (!match.Success)
        {
            throw new SyntaxErrorException($"Некорректный синтаксис директивы цикла: {foreachDirective}");
        }

        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    private static object? ResolvePropertyPath(string expressionPath, object? rootModel, Dictionary<string, object?> localScope)
    {
        var segments = expressionPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            return null;

        object? target;
        var startIdx = 0;

        if (localScope.TryGetValue(segments[0], out var scopedVal))
        {
            target = scopedVal;
            startIdx = 1;
        }
        else
        {
            target = rootModel;
        }

        for (var i = startIdx; i < segments.Length; i++)
        {
            if (target is null)
                return null;

            var targetType = target.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            var property = targetType.GetProperty(segments[i], flags);
            if (property is not null)
            {
                target = property.GetValue(target);
                continue;
            }

            var field = targetType.GetField(segments[i], flags);
            if (field is not null)
            {
                target = field.GetValue(target);
                continue;
            }

            return null;
        }

        return target;
    }

    private static bool CheckCondition(string rawCondition, object? rootModel, Dictionary<string, object?> localScope)
    {
        var isNegated = false;
        var cleanCondition = rawCondition.Trim();

        if (cleanCondition.StartsWith('!'))
        {
            isNegated = true;
            cleanCondition = cleanCondition[1..].Trim();
        }

        var resolved = ResolvePropertyPath(cleanCondition, rootModel, localScope);

        var truthy = resolved switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrWhiteSpace(s),
            int n => n > 0,
            long n => n > 0,
            short n => n > 0,
            double n => n > 0,
            float n => n > 0,
            decimal n => n > 0,
            _ => true
        };

        return isNegated ? !truthy : truthy;
    }

    private static string InterpolateVariables(string rawText, object? rootModel, Dictionary<string, object?> localScope)
    {
        return InterpolationRegex.Replace(rawText, match =>
        {
            var path = match.Groups[1].Value.Trim();
            var value = ResolvePropertyPath(path, rootModel, localScope);
            return value?.ToString() ?? string.Empty;
        });
    }

    private static void AdvanceToBlockEnd(List<TemplateToken> tokens, ref int index, bool stopAtElse)
    {
        var nestedDepth = 0;

        while (index < tokens.Count)
        {
            var currentType = tokens[index].Type;

            if (currentType is TokenType.If or TokenType.Foreach)
            {
                nestedDepth++;
            }
            else if (currentType is TokenType.EndIf or TokenType.EndFor)
            {
                if (nestedDepth == 0)
                    return;

                nestedDepth--;
            }
            else if (currentType == TokenType.Else && nestedDepth == 0 && stopAtElse)
            {
                return;
            }

            index++;
        }
    }

    private string ProcessTokens(
        List<TemplateToken> tokens,
        ref int index,
        object? dataModel,
        Dictionary<string, object?> localScope)
    {
        var builder = new StringBuilder();

        while (index < tokens.Count)
        {
            var token = tokens[index];

            if (token.Type is TokenType.EndIf or TokenType.Else or TokenType.EndFor)
            {
                return builder.ToString();
            }

            switch (token.Type)
            {
                case TokenType.Text:
                    builder.Append(InterpolateVariables(token.Content, dataModel, localScope));
                    index++;
                    break;

                case TokenType.If:
                {
                    var conditionExpr = ParseIfCondition(token.Content);
                    var conditionResult = CheckCondition(conditionExpr, dataModel, localScope);
                    index++;

                    if (conditionResult)
                    {
                        builder.Append(ProcessTokens(tokens, ref index, dataModel, localScope));

                        if (index < tokens.Count && tokens[index].Type == TokenType.Else)
                        {
                            index++;
                            AdvanceToBlockEnd(tokens, ref index, stopAtElse: false);
                        }
                    }
                    else
                    {
                        AdvanceToBlockEnd(tokens, ref index, stopAtElse: true);

                        if (index < tokens.Count && tokens[index].Type == TokenType.Else)
                        {
                            index++;
                            builder.Append(ProcessTokens(tokens, ref index, dataModel, localScope));
                        }
                    }

                    if (index < tokens.Count && tokens[index].Type == TokenType.EndIf)
                    {
                        index++;
                    }

                    break;
                }

                case TokenType.Foreach:
                {
                    var (itemVar, collectionExpr) = ParseForeachHeader(token.Content);
                    index++;

                    var loopStartIdx = index;
                    var rawCollection = ResolvePropertyPath(collectionExpr, dataModel, localScope);

                    if (rawCollection is IEnumerable enumerable and not string)
                    {
                        foreach (var element in enumerable)
                        {
                            var childScope = new Dictionary<string, object?>(localScope, StringComparer.OrdinalIgnoreCase)
                            {
                                [itemVar] = element
                            };

                            var branchIdx = loopStartIdx;
                            builder.Append(ProcessTokens(tokens, ref branchIdx, dataModel, childScope));
                        }
                    }

                    index = loopStartIdx;
                    AdvanceToBlockEnd(tokens, ref index, stopAtElse: false);

                    if (index < tokens.Count && tokens[index].Type == TokenType.EndFor)
                    {
                        index++;
                    }

                    break;
                }
            }
        }

        return builder.ToString();
    }
}
