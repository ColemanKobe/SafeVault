using NUnit.Framework;
using SafeVault.Models;
using System.ComponentModel.DataAnnotations;

[TestFixture]
public class TestInputValidation 
{
    [Test]
    public void ValidUsername_ShouldPassValidation()
    {
        var model = new RegisterModel
        {
            Username = "validuser123",
            Email = "test@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void Username_WithInvalidCharacters_ShouldFailValidation()
    {
        var model = new RegisterModel
        {
            Username = "user@name!", // Contains invalid characters
            Email = "test@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Any(r => r.MemberNames.Contains("Username")), Is.True);
    }

    [Test]
    public void Username_TooShort_ShouldFailValidation()
    {
        var model = new RegisterModel
        {
            Username = "ab", // Too short
            Email = "test@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Any(r => r.MemberNames.Contains("Username")), Is.True);
    }

    [Test]
    public void Password_WithoutRequiredComplexity_ShouldFailValidation()
    {
        var model = new RegisterModel
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "simplepass", // Missing complexity requirements
            ConfirmPassword = "simplepass"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Any(r => r.MemberNames.Contains("Password")), Is.True);
    }

    [Test]
    public void Password_MismatchConfirmation_ShouldFailValidation()
    {
        var model = new RegisterModel
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "SecureP@ss123",
            ConfirmPassword = "DifferentP@ss123"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Any(r => r.MemberNames.Contains("ConfirmPassword")), Is.True);
    }

    [Test]
    public void Email_InvalidFormat_ShouldFailValidation()
    {
        var model = new RegisterModel
        {
            Username = "testuser",
            Email = "invalid-email", // Invalid email format
            Password = "SecureP@ss123",
            ConfirmPassword = "SecureP@ss123"
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Any(r => r.MemberNames.Contains("Email")), Is.True);
    }

    [Test]
    public void LoginModel_ValidInput_ShouldPassValidation()
    {
        var model = new LoginModel
        {
            UsernameOrEmail = "testuser",
            Password = "SecureP@ss123",
            RememberMe = false
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    [Test]
    public void LoginModel_EmptyCredentials_ShouldFailValidation()
    {
        var model = new LoginModel
        {
            UsernameOrEmail = "",
            Password = "",
            RememberMe = false
        };

        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.That(isValid, Is.False);
        Assert.That(results.Count, Is.GreaterThanOrEqualTo(2)); // Should have errors for both username and password
    }
}