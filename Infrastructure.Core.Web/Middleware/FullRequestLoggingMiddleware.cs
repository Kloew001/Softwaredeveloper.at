using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Middleware;

public static class FullRequestLoggingBuilderExtensions
{
    public static IApplicationBuilder UseFullRequestLogging(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<FullRequestLoggingMiddleware>();
    }
}

public class FullRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FullRequestLoggingMiddleware> _logger;
    private readonly FullRequestLoggingConfiguration _configuration;

    public FullRequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<FullRequestLoggingMiddleware> logger,
        FullRequestLoggingConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!_configuration.Enabled)
        {
            await _next(context);
            return;
        }

        var requestBody = string.Empty;
        if (_configuration.IncludeBody)
        {
            requestBody = await ReadRequestBodySafeAsync(context.Request, _configuration.MaxBodyLength);
        }

        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        var elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

        var responseBody = string.Empty;
        if (_configuration.IncludeBody)
        {
            responseBody = ReadResponseBodyDecoded(context.Response, responseBodyStream, _configuration.MaxBodyLength);
        }

        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalBodyStream);

        var request = context.Request;

        var headers = request.Headers.ToDictionary(h => h.Key, h => SanitizeValue(h.Value.ToString()));
        if (_configuration.SanitizeSensitiveHeaders)
        {
            foreach (var key in new[] { "Authorization" })
            {
                if (headers.ContainsKey(key))
                    headers[key] = "***";
            }
        }

        var query = request.Query.ToDictionary(q => q.Key, q => SanitizeValue(q.Value.ToString()));
        var routeValues = request.RouteValues.ToDictionary(kvp => kvp.Key, kvp => SanitizeValue(kvp.Value?.ToString()));

        var logMessage = new StringBuilder();

        logMessage.AppendLine("FULL REQUEST/RESPONSE DUMP");
        logMessage.AppendLine($"Method: {request.Method}");
        logMessage.AppendLine($"Path: {request.Path}");
        logMessage.AppendLine($"StatusCode: {context.Response.StatusCode}");
        logMessage.AppendLine($"ExecutionTimeMs: {elapsedMs:0.00} ms");
        logMessage.AppendLine();
        logMessage.AppendLine(FormatObject("Headers", headers));
        logMessage.AppendLine();
        logMessage.AppendLine(FormatObject("Query", query));
        logMessage.AppendLine();
        logMessage.AppendLine(FormatObject("RouteValues", routeValues));
        logMessage.AppendLine();
        logMessage.AppendLine(FormatSection("RequestBody", requestBody));
        logMessage.AppendLine();
        logMessage.AppendLine(FormatSection("ResponseBody", responseBody));

        _logger.LogWarning(logMessage.ToString());

        context.Response.Body = originalBodyStream;
    }
    private string FormatSection(string title, string content)
    {
        return $"{title}:\n{(content ?? "(null)")}";
    }

    private string FormatObject(string title, object obj)
    {
        var json = obj.ToJson(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        });

        return $"{title}:\n{json}";
    }

    private string SanitizeValue(string? input, int maxLength = 1024)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        if (input.Length > maxLength)
        {
            input = input.Substring(0, maxLength) + "...(truncated)";
        }

        var sb = new StringBuilder(input.Length);

        foreach (var ch in input)
        {
            if (ch == '\r' || ch == '\n')
            {
                sb.Append(' ');
                continue;
            }

            if (ch == '\u001b' || ch == '\0')
            {
                continue;
            }

            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            switch (cat)
            {
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                    continue;
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    private async Task<string> ReadRequestBodySafeAsync(HttpRequest request, int maxLength)
    {
        if (request.ContentLength == null || request.ContentLength == 0)
            return null;

        request.EnableBuffering();

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        if (body.Length > maxLength)
            body = body.Substring(0, maxLength) + "...(truncated)";

        return SanitizeValue(body, maxLength); ;
    }

    private string ReadResponseBodyDecoded(HttpResponse response, MemoryStream buffer, int maxLength)
    {
        if (!IsTextContentType(response.ContentType))
            return "(non-text content omitted)";

        buffer.Position = 0;
        var data = buffer.ToArray();

        response.Headers.TryGetValue("Content-Encoding", out var contentEncoding);
        var encoding = contentEncoding.ToString();

        string text;

        if (encoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            using var compressedStream = new MemoryStream(data);
            using var gzip = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            text = reader.ReadToEnd();
        }
        else if (encoding.Contains("br", StringComparison.OrdinalIgnoreCase))
        {
            using var compressedStream = new MemoryStream(data);
            using var brotli = new BrotliStream(compressedStream, CompressionMode.Decompress);
            using var reader = new StreamReader(brotli, Encoding.UTF8);
            text = reader.ReadToEnd();
        }
        else
        {
            text = Encoding.UTF8.GetString(data);
        }

        return FormatResponseBody(text, maxLength);
    }

    private string FormatResponseBody(string body, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        // Versuchen JSON einzurücken
        try
        {
            using var doc = JsonDocument.Parse(body);
            var pretty = System.Text.Json.JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            if (pretty.Length > maxLength)
                pretty = pretty.Substring(0, maxLength) + "...(truncated)";

            return pretty;
        }
        catch
        {
            // body ist kein JSON → einfach normal ausgeben
        }

        if (body.Length > maxLength)
            body = body.Substring(0, maxLength) + "...(truncated)";

        return body;
    }

    private bool IsTextContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return contentType.StartsWith("text/")
            || contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
    }
}