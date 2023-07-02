using FluentValidation;
using Microsoft.Extensions.Options;

namespace WebApi.Validation;

public static class OptionsBuilderExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(
        this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
    {
        if (optionsBuilder is null)
        {
            throw new ArgumentNullException(nameof(optionsBuilder));
        }

        optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
            sp => new FluentValidationOptions<TOptions>(
                optionsBuilder.Name,
                sp.GetRequiredService<IValidator<TOptions>>()));
        return optionsBuilder;
    }
}
