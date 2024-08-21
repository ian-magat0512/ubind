// <copyright file="DateTimeZoneConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.JsonConverters;

using System.Text.Json;
using FluentAssertions;
using NodaTime;
using UBind.Application.JsonConverters;
using Xunit;

public class DateTimeZoneConverterTests
{
    private readonly JsonSerializerOptions options;

    public DateTimeZoneConverterTests()
    {
        this.options = new JsonSerializerOptions
        {
            Converters = { new DateTimeZoneConverter() },
        };
    }

    [Fact]
    public void Can_Serialize_And_Deserialize_DateTimeZone_String()
    {
        var timeZone = DateTimeZoneProviders.Tzdb["Australia/Sydney"];
        var json = JsonSerializer.Serialize(timeZone, this.options);
        var deserializedTimeZone = JsonSerializer.Deserialize<DateTimeZone>(json, this.options);
        deserializedTimeZone.Should().NotBeNull();
        deserializedTimeZone?.Id.Should().Be(timeZone.Id);
    }

    [Fact]
    public void Can_Serialize_And_Deserialize_DateTimeZone_Object()
    {
        var timeZone = DateTimeZoneProviders.Tzdb["Australia/Sydney"];
        var json = JsonSerializer.Serialize(new { id = timeZone.Id }, this.options);
        var deserializedTimeZone = JsonSerializer.Deserialize<DateTimeZone>(json, this.options);
        deserializedTimeZone.Should().NotBeNull();
        deserializedTimeZone?.Id.Should().Be(timeZone.Id);
    }
}
