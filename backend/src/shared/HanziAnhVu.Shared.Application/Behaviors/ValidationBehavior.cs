using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HanziAnhVu.Shared.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count > 0)
        {
            var validationErrors = failures.Select(failure => new
            {
                failure.PropertyName,
                failure.ErrorMessage,
                failure.AttemptedValue,
                failure.ErrorCode,
                failure.Severity
            }).ToList();

            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {@ValidationErrors}",
                typeof(TRequest).Name,
                validationErrors);

            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}