namespace Lesson.Infrastructure.Repository;

public abstract class LessonRepositoryBase(ILogger logger)
{
    protected ILogger Logger { get; } = logger;

    protected Task ExecuteAsync(
        Action operation,
        string dbErrorLogMessage,
        string unexpectedErrorLogMessage,
        string repositoryErrorMessage,
        params object?[] logArgs)
    {
        try
        {
            operation();
            return Task.CompletedTask;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, dbErrorLogMessage, logArgs);
            throw new RepositoryException(repositoryErrorMessage, ex);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, unexpectedErrorLogMessage, logArgs);
            throw;
        }
    }

    protected async Task ExecuteAsync(
        Func<Task> operation,
        string dbErrorLogMessage,
        string unexpectedErrorLogMessage,
        string repositoryErrorMessage,
        params object?[] logArgs)
    {
        try
        {
            await operation();
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, dbErrorLogMessage, logArgs);
            throw new RepositoryException(repositoryErrorMessage, ex);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, unexpectedErrorLogMessage, logArgs);
            throw;
        }
    }

    protected async Task<TResult> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        string dbErrorLogMessage,
        string unexpectedErrorLogMessage,
        string repositoryErrorMessage,
        params object?[] logArgs)
    {
        try
        {
            return await operation();
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, dbErrorLogMessage, logArgs);
            throw new RepositoryException(repositoryErrorMessage, ex);
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Operation cancelled");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, unexpectedErrorLogMessage, logArgs);
            throw;
        }
    }
}