using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Zircon.AspNetCore.MediatR.Behaviors;

public class ModelValidationBehaviour<TRequest, TResult>
    (IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResult>
    where TRequest : IRequest<TResult>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResult> Handle(TRequest request,
        RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = _validators.Select(v => v.Validate(context)).ToList();
        var groupedValidationFailures = validationResults.SelectMany(v => v.Errors)
            .GroupBy(e => e.PropertyName)
            .Select(g => new
            {
                PropertyName = g.Key,
                ValidationFailures = g.Select(v => new { v.ErrorMessage })
            }).ToList();

        if (groupedValidationFailures.Count != 0)
        {
            var validationProblemsDictionary = new Dictionary<string, string[]>();
            foreach (var group in groupedValidationFailures)
            {
                var errorMessages = group.ValidationFailures.Select(v => v.ErrorMessage);
                validationProblemsDictionary.Add(group.PropertyName, errorMessages.ToArray());
            }

            return (TResult)Results.ValidationProblem(validationProblemsDictionary);
        }

        return await next();
    }
}
