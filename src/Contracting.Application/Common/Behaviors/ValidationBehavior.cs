using ErrorOr;
using FluentValidation;
using MediatR;

namespace Contracting.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                // Check if TResponse is ErrorOr<T>
                var responseType = typeof(TResponse);
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(ErrorOr<>))
                {
                    // Create validation errors for ErrorOr
                    var errors = failures
                        .Select(failure => Error.Validation(
                            code: failure.PropertyName,
                            description: failure.ErrorMessage))
                        .ToList();

                    // Get the generic argument (T in ErrorOr<T>)
                    var resultType = responseType.GetGenericArguments()[0];
                    
                    // Create ErrorOr<T> from errors using reflection
                    var errorOrType = typeof(ErrorOr<>).MakeGenericType(resultType);
                    var fromMethod = errorOrType.GetMethod("From", new[] { typeof(List<Error>) });
                    
                    if (fromMethod != null)
                    {
                        return (TResponse)fromMethod.Invoke(null, new object[] { errors })!;
                    }
                }
                else
                {
                    // Fallback to Result<T> pattern for backward compatibility
                    var dict = failures
                        .GroupBy(f => f.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

                    var method = responseType.GetMethod("Validation", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (method != null)
                    {
                        return (TResponse)method.Invoke(null, new object[] { dict })!;
                    }
                }
            }
        }

        return await next();
    }
}
