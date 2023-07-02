using FluentValidation;
using Microsoft.Extensions.Options;

namespace WebApi.Validation;

public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IValidator<TOptions> _validator;

    public FluentValidationOptions(string name, IValidator<TOptions> validator)
    {
        Name = name;
        _validator = validator;
    }

    public string Name { get; }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (Name != null && Name != name)
        {
            return ValidateOptionsResult.Skip;
        }

        var validationResult = _validator.Validate(options);
        if (validationResult.IsValid)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = validationResult.Errors.Select(x => $"Options validation failed for [{x.PropertyName}] with error: [{x.ErrorMessage}]");
        return ValidateOptionsResult.Fail(errors);
    }
}
