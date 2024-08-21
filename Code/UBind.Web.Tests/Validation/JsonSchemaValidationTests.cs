// <copyright file="JsonSchemaValidationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.Validation;

using FluentAssertions;
using Newtonsoft.Json.Schema;
using Xunit;

/// <summary>
/// Defines the <see cref="JsonSchemaValidationTests" />.
/// </summary>
public class JsonSchemaValidationTests
{
    /// <summary>
    /// Tests if JSON schema has valid content.
    /// </summary>
    /// <param name="path">The path of JSON schema.<see cref="string"/>.</param>
    [Theory]
    [InlineData("automation-data.example.1.0.0.json")]
    [InlineData("automation-data.schema.1.0.0.json")]
    [InlineData("automations.example.1.0.0.json")]
    [InlineData("automations.schema.1.0.0.json")]
    [InlineData("outbound-email-servers.example.1.0.0.json")]
    [InlineData("outbound-email-servers.schema.1.0.0.json")]
    [InlineData("serialised-entity.schema.1.0.0.json")]
    public void JsonSchemaValidation_ValidSchema_Passes(string path)
    {
        path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schemas", "automations", path);
        string schemaJson = File.ReadAllText(path);
        JSchema schema = new JSchema();
        schema.Invoking(s => s = JSchema.Parse(schemaJson))
            .Should().NotThrow<JSchemaReaderException>();
        schema.Should().NotBeNull();
    }
}
