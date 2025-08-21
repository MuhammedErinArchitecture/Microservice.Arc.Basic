using FluentAssertions;
using UserService.Api.Contracts;
using UserService.Api.Validation;

namespace UserService.Tests;

public class UserValidatorsTests
{
    [Fact]
    public void CreateValidator_Should_Fail_On_Invalid_Email()
    {
        var v = new UserCreateRequestValidator();
        var r = v.Validate(new UserCreateRequest("A", "B", "not-an-email", "Author"));
        r.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateValidator_Should_Pass_On_Valid_Payload()
    {
        var v = new UserCreateRequestValidator();
        var r = v.Validate(new UserCreateRequest("Ada", "Lovelace", "ada@example.com", "Author"));
        r.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_Should_Require_Fields()
    {
        var v = new UserUpdateRequestValidator();
        var ok = v.Validate(new UserUpdateRequest("First", "Last", "Author", "Active"));
        var bad = v.Validate(new UserUpdateRequest("", "", "", ""));
        ok.IsValid.Should().BeTrue();
        bad.IsValid.Should().BeFalse();
    }
}