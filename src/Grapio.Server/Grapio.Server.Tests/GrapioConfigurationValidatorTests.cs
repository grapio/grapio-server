using FluentValidation.TestHelper;

namespace Grapio.Server.Tests;

public class GrapioConfigurationValidatorTests
{
    private readonly GrapioConfigurationValidator _validator = new();

    [Fact]
    public void Validate_should_return_validation_error_for_an_invalid_connection_string()
    {
        var config = new GrapioConfiguration
        {
            ConnectionString = "invalid-connection-string"
        };
            
        var validationResult = _validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.ConnectionString)
            .WithErrorMessage("Connection string is invalid.");
    }
}