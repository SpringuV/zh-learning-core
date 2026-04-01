using HanziAnhVu.Shared.Application;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Infrastructure;

public class LoggingAdapter<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public LoggingAdapter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<T>();
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }
}
