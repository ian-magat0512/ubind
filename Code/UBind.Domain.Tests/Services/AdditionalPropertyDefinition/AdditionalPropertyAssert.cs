// <copyright file="AdditionalPropertyAssert.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyDefinition
{
    using FluentAssertions;
    using UBind.Domain;

    public class AdditionalPropertyAssert
    {
        public static void AssertWhenPropertyNameAlreadyExists(
            Error error)
        {
            error.Code.Should().Be("additionalproperty.name.in.use");
        }

        public static void AssertWhenAliasExists(
            Error error)
        {
            error.Code.Should().Be("additionalproperty.alias.in.use");
        }

        public static void AssertWhenRecordIsNotFound(Error exception)
        {
            exception.Code.Should().Be("additionalproperty.id.not.found");
        }
    }
}
