using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationFailures = await ValidateRequestAsync(request);

        if (validationFailures.Length == 0)
        {
            return await next();
        }

        return HandleValidationFailures(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateRequestAsync(TRequest request)
    {
        if (!validators.Any())
        {
            return [];
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));

        return validationResults
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)
            .ToArray();
    }

    private static TResponse HandleValidationFailures(ValidationFailure[] validationFailures)
    {
        if (IsGenericResultType(out var resultType))
        {
            return CreateGenericValidationResult(validationFailures, resultType!);
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
        }

        throw new ValidationException(validationFailures);
    }

    private static bool IsGenericResultType(out Type? resultType)
    {
        resultType = null;

        if (!typeof(TResponse).IsGenericType || typeof(TResponse).GetGenericTypeDefinition() != typeof(Result<>))
        {
            return false;
        }

        resultType = typeof(TResponse).GetGenericArguments()[0];
        return true;
    }

    private static TResponse CreateGenericValidationResult(ValidationFailure[] validationFailures, Type resultType)
    {
        var failureMethod = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod(nameof(Result<object>.ValidationFailure));

        if (failureMethod is null)
        {
            throw new InvalidOperationException("Validation failure method not found.");
        }

        return (TResponse)failureMethod.Invoke(null, new object[] { CreateValidationError(validationFailures) })!;
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures)
    {
        var errors = validationFailures
            .Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage))
            .ToArray();

        return new ValidationError(errors);
    }
}
