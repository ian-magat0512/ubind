// <copyright file="PersonInformationHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers;

using FluentAssertions;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using Xunit;

/// <summary>
/// String extensions unit tests.
/// </summary>
public class PersonInformationHelperTests
{

    /// <summary>
    /// Test email masking.
    /// </summary>
    /// <param name="input">The email address.</param>
    /// <param name="expectedOutput">The masked email address.</param>
    [Theory]
    [InlineData("leon.tayson@ubind.io", "l****n@u****.io")]
    [InlineData("jimsmith@company.au", "j****h@c****.au")]
    [InlineData("2@2", "2****@2****")]
    [InlineData("a@a.", "a****@a****.")]
    [InlineData("a@a.b", "a****@a****.b")]
    [InlineData("ab@c.d", "a****b@c****.d")]
    [InlineData("a.b@cd.e", "a****b@c****.e")]
    [InlineData("@b", "@****b")]
    [InlineData("a@", "a****@")]
    [InlineData("a", "a****")]
    [InlineData("ab", "a****b")]
    [InlineData("   ", "")]
    [InlineData(" \t  \t ", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void IsEmail_MaskedProperly(string input, string expectedOutput)
    {
        // Act
        var maskedEmail = PersonInformationHelper.GetMaskedEmail(input);

        // Assert
        maskedEmail.Should().Be(expectedOutput);
    }

    /// <summary>
    /// Test email masking.
    /// </summary>
    /// <param name="input">The email address.</param>
    /// <param name="expectedOutput">The masked email address.</param>
    [Theory]
    [InlineData("leon.tayson@ubind.io", "l***n", "@u***d.io")]
    [InlineData("jimsmith@company.au", "j***h", "@c***y.au")]
    [InlineData("2@2", "2***", "@2***")]
    [InlineData("a@a.", "a***", "@a***.")]
    [InlineData("a@a.b", "a***", "@a***.b")]
    [InlineData("ab@c.d", "a***b", "@c***.d")]
    [InlineData("a.b@cd.e", "a***b", "@c***d.e")]
    [InlineData("@b", "", "@***b")]
    [InlineData("a@", "a***", "@")]
    [InlineData("a", "a***", null)]
    [InlineData("ab", "a***b", null)]
    [InlineData("   ", "", null)]
    [InlineData(" \t  \t ", "", null)]
    [InlineData("", "", null)]
    [InlineData(null, "", null)]
    public void IsEmail_MaskedWithHashingProperly(string input, string expectedStartString, string expectedEndString)
    {
        // Act
        var maskedEmail = PersonInformationHelper.GetMaskedEmailWithHashing(input);

        // Assert
        maskedEmail.Should().StartWith(expectedStartString);
        if (expectedEndString != null)
        {
            maskedEmail.Should().EndWith(expectedEndString);
        }
    }

    /// <summary>
    /// Test name masking with hash.
    /// </summary>
    /// <param name="input">The name.</param>
    /// <param name="expectedOutput">The masked name.</param>
    [Theory]
    [InlineData("Leon Tayson", "L***n_")]
    [InlineData("Jim Smith", "J***h_")]
    [InlineData("Jim S", "J***S_")]
    [InlineData("Madonna", "M***")]
    [InlineData("M", "M***")]
    [InlineData("*****", "****")]
    [InlineData("#$%*( %^*& %^&*", "#****")]
    [InlineData("    ", "")]
    [InlineData("  \t  ", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void IsName_MaskedWithHashProperly(string input, string expectedStartOutput)
    {
        // Act
        var maskedName = PersonInformationHelper.GetMaskedNameWithHashing(input);

        // Assert
        maskedName.Should().StartWith(expectedStartOutput);
    }
}
