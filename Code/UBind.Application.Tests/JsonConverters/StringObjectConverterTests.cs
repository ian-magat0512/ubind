// <copyright file="StringObjectConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.JsonConverters;

using Newtonsoft.Json;
using UBind.Application.JsonConverters;
using UBind.Domain.ValueTypes;
using Xunit;
public class StringObjectConverterTests
{
    [Fact]
    public void ReadJson_WhenTypeIsFileName_CallsConvertMethodWithCorrectValue()
    {
        // Arrange
        var converter = new StringObjectConverter<FileName>();
        var json = "{\"Value\": \"example.txt\"}";
        var reader = new JsonTextReader(new StringReader(json));

        // Manually advance the reader to simulate the StartObject token
        reader.Read();

        // Act
        var result = converter.ReadJson(reader, typeof(FileName), null, false, null);

        // Assert
        Assert.Equal(new FileName("example.txt"), result);
    }

    [Fact]
    public void WriteJson_WhenValueIsFileName_WritesCorrectStringValue()
    {
        // Arrange
        var converter = new StringObjectConverter<FileName>();
        var stringWriter = new StringWriter();
        var writer = new JsonTextWriter(stringWriter);
        var fileName = new FileName("example.docx");

        // Act
        converter.WriteJson(writer, fileName, null);

        // Assert
        Assert.Equal("\"example.docx\"", stringWriter.ToString());
    }
}
