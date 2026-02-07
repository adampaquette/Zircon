using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace Zircon.AspNetCore.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates incoming messages using FluentValidation.
/// Returns a validation problem result if validation fails.
/// </summary>
public sealed class ModelValidationBehaviour<TMessage, TResponse>(
    IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);

        var validationResults = validators.Select(v => v.Validate(context)).ToList();
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

            return (TResponse)Results.ValidationProblem(validationProblemsDictionary);
        }

        return await next(message, cancellationToken);
    }
}
