using System.Net;
using System.Text.Json;
using TodoListApp.Services;

namespace TodoListApp.WebApi.Logger;

internal class ExceptionHandling
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandling> _logger;

    public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
    {
        this._next = next;
        this._logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await this._next(context);
        }
        catch (OperationCanceledException ex)
        {
            await HandleExceptionAsync(context, ex, this._logger);
        }
        catch (TimeoutException ex)
        {
            await HandleExceptionAsync(context, ex, this._logger);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(context, ex, this._logger);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, this._logger);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            await HandleExceptionAsync(context, ex, this._logger);
        }
    }

    private static readonly Action<ILogger, string, Exception?> _logUnhandledException =
       LoggerMessage.Define<string>(
           LogLevel.Error,
           new EventId(1000, nameof(ExceptionHandling)),
           "Unhandled exception: {Message}");


    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
    {
        HttpStatusCode status;
        string message;

        switch (ex)
        {
            case NotFoundException:
            case KeyNotFoundException:
                status = HttpStatusCode.NotFound;
                message = ex.Message;
                break;

            case UnauthorizedAccessException:
                status = HttpStatusCode.Forbidden;
                message = ex.Message;
                break;

            case ArgumentException:
                status = HttpStatusCode.BadRequest;
                message = ex.Message;
                break;

            case TimeoutException:
                status = HttpStatusCode.GatewayTimeout;
                message = "Operation timed out.";
                break;

            case OperationCanceledException:
                status = (HttpStatusCode)499;
                message = "Request was canceled by the client.";
                break;

            default:
                status = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                break;
        }

        _logUnhandledException(logger, ex.Message, ex);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var result = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(result);
    }
}
