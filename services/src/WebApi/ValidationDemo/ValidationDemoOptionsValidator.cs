using FluentValidation;

namespace WebApi.ValidationDemo
{
    public class ValidationDemoOptionsValidator : AbstractValidator<ValidationDemoOptions>
    {
        public ValidationDemoOptionsValidator()
        {
            RuleFor(o => o.UnixTimeSeconds).Equal(o => o.IsoDate!.Value.ToUnixTimeSeconds()).When(o => o.IsoDate != null);
            RuleFor(o => o.IsoDate).Equal(o => DateTimeOffset.FromUnixTimeSeconds(o.UnixTimeSeconds!.Value)).When(o => o.UnixTimeSeconds != null);
        }
    }
}
