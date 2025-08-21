using System;
using ContentService.Api.Contracts;
using ContentService.Api.Validation;
using FluentAssertions;
using Xunit;

namespace ContentService.Tests;

public class ContentValidatorsTests
{
    [Fact]
    public void CreateValidator_Should_Fail_When_Title_Too_Short()
    {
        var v = new ContentCreateRequestValidator();
        var r = v.Validate(new ContentCreateRequest("aa", "body long enough ...", Guid.NewGuid()));
        r.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateValidator_Should_Fail_When_Body_Too_Short()
    {
        var v = new ContentCreateRequestValidator();
        var r = v.Validate(new ContentCreateRequest("Valid Title", "short", Guid.NewGuid()));
        r.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateValidator_Should_Pass_When_Valid()
    {
        var v = new ContentCreateRequestValidator();
        var r = v.Validate(new ContentCreateRequest("Valid Title", "body long enough ...", Guid.NewGuid()));
        r.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_Should_Enforce_Status_Whitelist()
    {
        var v = new ContentUpdateRequestValidator();
        var ok = v.Validate(new ContentUpdateRequest("Title", "body long enough ...", "Published"));
        var bad = v.Validate(new ContentUpdateRequest("Title", "body long enough ...", "Whatever"));
        ok.IsValid.Should().BeTrue();
        bad.IsValid.Should().BeFalse();
    }
}