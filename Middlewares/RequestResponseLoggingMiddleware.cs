using LoggingModule.Models.LogModels;
using LoggingModule.Helpers;

using Microsoft.AspNetCore.Mvc.Controllers;

using Newtonsoft.Json;

using System.Diagnostics;
using System.Security.Claims;

namespace LoggingModule.Middlewares;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"test test");
        var endpoint = context.GetEndpoint();
        var routeData = context.GetRouteData();
        var controllerName = routeData.Values["controller"]?.ToString();

        Type? controllerType = null;

        // Fallback to reflection if endpoint is not resolved
        if (!string.IsNullOrEmpty(controllerName))
        {
            controllerType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    t.Name.Equals(controllerName + "Controller", StringComparison.OrdinalIgnoreCase) &&
                    typeof(IApiLoggable).IsAssignableFrom(t));
        }

        if (controllerType == null)
        {
            await _next(context);
            return;
        }

        var logEntry = new HttpRequestLog();
        Exception capturedException = null;

        HttpRequest request = context.Request;
        logEntry.Method = request.Method;
        logEntry.Path = request.Path + request.PathBase;
        logEntry.Hostname = request.Host.Value;
        logEntry.QueryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
        logEntry.IpAddress = context.Connection.RemoteIpAddress?.ToString();
        logEntry.UserAgent = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (endpoint != null)
        {
            var cad = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (cad != null)
            {
                // e.g. "App.Http.Controllers.Admin.ProviderUpdatesController@ShowUpdateRequest"
                logEntry.ControllerAction = $"{cad.ControllerTypeInfo.FullName}@{cad.ActionName}";
            }
        }

        request.EnableBuffering();
        if (request.ContentType != null && request.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            var formFields = new Dictionary<string, string>();
            var filesInfo = new List<object>();

            if (!request.HasFormContentType)
            {
                // You have to read it first to enable accessing form
                await request.ReadFormAsync();
            }

            foreach (var field in request.Form.Where(f => f.Value.Count > 0))
            {
                formFields[field.Key] = field.Value.ToString();
            }

            foreach (var file in request.Form.Files)
            {
                filesInfo.Add(new
                {
                    file.Name,
                    file.FileName,
                    file.Length,
                    file.ContentType
                });
            }

            logEntry.RequestBody = JsonConvert.SerializeObject(new
            {
                Fields = formFields,
                Files = filesInfo
            }, Formatting.Indented);
        }
        else
        {
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                logEntry.RequestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
        }

        logEntry.RequestHeaders = JsonConvert.SerializeObject(
            request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

        var originalBodyStream = context.Response.Body;
        using var responseBuffer = new MemoryStream();

        var stopwatch = Stopwatch.StartNew();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            context.Response.StatusCode = 500; // Optional: ensure status code is set
        }
        finally
        {
            stopwatch.Stop();
            logEntry.Elapsed = stopwatch.Elapsed.TotalSeconds;
            logEntry.ResponseStatusCode = context.Response.StatusCode;

            responseBuffer.Seek(0, SeekOrigin.Begin);
            logEntry.ResponseBody = await new StreamReader(responseBuffer).ReadToEndAsync();
            logEntry.ResponseHeaders = JsonConvert.SerializeObject(
                context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

            responseBuffer.Seek(0, SeekOrigin.Begin);
            await responseBuffer.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
            logEntry.MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1_048_576;
            using (_logger.BeginScope(new Dictionary<string, object> { { "FilterType", "dummy_filter" } }))
            {
                if (capturedException != null)
                {
                    logEntry.ExceptionDetails = capturedException.ToString();

                    _logger.LogError("HTTP Log {@Method} {@Path} {@QueryString} {@Elapsed} {@RequestBody} {@ResponseBody} {@RequestHeaders} {@ResponseHeaders} {@ResponseStatusCode} {@ExceptionDetails} {@ControllerAction} {@Hostname} {@MemoryUsageBytes} {@IPAddress} {@UserAgent}",
                        logEntry.Method,
                        logEntry.Path,
                        logEntry.QueryString,
                        logEntry.Elapsed,
                        logEntry.RequestBody,
                        logEntry.ResponseBody,
                        logEntry.RequestHeaders,
                        logEntry.ResponseHeaders,
                        logEntry.ResponseStatusCode,
                        logEntry.ExceptionDetails,
                        logEntry.ControllerAction,
                        logEntry.Hostname,
                        logEntry.MemoryUsage,
                        logEntry.IpAddress,
                        logEntry.UserAgent);

                    throw capturedException; // rethrow after logging
                }
                else
                {
                    _logger.LogInformation("HTTP Log {@Method} {@Path} {@QueryString} {@Elapsed} {@RequestBody} {@ResponseBody} {@RequestHeaders} {@ResponseHeaders} {@ResponseStatusCode} {@ControllerAction} {@Hostname} {@MemoryUsage} {@IpAddress} {@UserAgent}",
                        logEntry.Method,
                        logEntry.Path,
                        logEntry.QueryString,
                        logEntry.Elapsed,
                        logEntry.RequestBody,
                        logEntry.ResponseBody,
                        logEntry.RequestHeaders,
                        logEntry.ResponseHeaders,
                        logEntry.ResponseStatusCode,
                        logEntry.ControllerAction,
                        logEntry.Hostname,
                        logEntry.MemoryUsage,
                        logEntry.IpAddress,
                        logEntry.UserAgent);
                }
            }

        }
    }
}
