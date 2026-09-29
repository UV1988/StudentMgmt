using StudentManager.Api.Dtos;
using StudentManager.Api.Validation;

namespace StudentManager.Tests;

public class CreateStudentRequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private readonly CreateStudentRequestValidator _validator = new(new FixedTimeProvider(Now));

    private static CreateStudentRequest Valid() =>
        new("Asha Rao", new DateOnly(2000, 1, 15), "+91 98765 43210");

    [Fact]
    public void Valid_request_passes() =>
        Assert.True(_validator.Validate(Valid()).IsValid);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Blank_name_fails(string name)
    {
        var result = _validator.Validate(Valid() with { Name = name });
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Name_longer_than_100_characters_fails()
    {
        var result = _validator.Validate(Valid() with { Name = new string('a', 101) });
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Date_of_birth_in_the_future_fails()
    {
        var result = _validator.Validate(Valid() with { DateOfBirth = new DateOnly(2026, 9, 29) });
        Assert.Contains(result.Errors, e => e.PropertyName == "DateOfBirth");
    }

    [Fact]
    public void Date_of_birth_today_is_allowed()
    {
        var result = _validator.Validate(Valid() with { DateOfBirth = new DateOnly(2026, 9, 28) });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Date_of_birth_more_than_120_years_ago_fails()
    {
        var result = _validator.Validate(Valid() with { DateOfBirth = new DateOnly(1900, 1, 1) });
        Assert.Contains(result.Errors, e => e.PropertyName == "DateOfBirth");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12345")]
    [InlineData("+")]
    [InlineData("123456789012345678901")]
    public void Invalid_phone_fails(string phone)
    {
        var result = _validator.Validate(Valid() with { Phone = phone });
        Assert.Contains(result.Errors, e => e.PropertyName == "Phone");
    }

    [Theory]
    [InlineData("+91 98765 43210")]
    [InlineData("9876543210")]
    [InlineData("020-2555-1234")]
    public void Valid_phone_passes(string phone)
    {
        var result = _validator.Validate(Valid() with { Phone = phone });
        Assert.True(result.IsValid);
    }
}
