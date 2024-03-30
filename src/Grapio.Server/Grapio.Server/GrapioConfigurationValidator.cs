using FluentValidation;

namespace Grapio.Server;

public class GrapioConfigurationValidator: AbstractValidator<GrapioConfiguration>
{
    public GrapioConfigurationValidator()
    {
        RuleFor(c => c.ConnectionString)
            .NotEmpty()
            .Must(v => v.Contains("DataSource") || v.Contains("Data Source") || v.Contains("Filename"))
            .WithMessage("Connection string is invalid.");
    }
}
