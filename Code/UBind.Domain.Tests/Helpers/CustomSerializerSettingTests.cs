// <copyright file="CustomSerializerSettingTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Events
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using Xunit;

    public class CustomSerializerSettingTests
    {
        [Fact]
        public void CustomSerializerSetting_serializeLocaDate_WorksWithOldLocalDateFormat()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDate = new LocalDate(2025, 12, 2);
            var json = "{\"$type\":\"UBind.Domain.Tests.Events.NodaTimeExampleModel, UBind.Domain.Tests\"," +
                       "\"localDate\":{\"$type\":\"NodaTime.LocalDate, NodaTime\"," +
                       "\"year\":" + model.LocalDate.Value.Year + ",\"month\":" + model.LocalDate.Value.Month +
                       ",\"day\":" + model.LocalDate.Value.Day + ",\"calendar\":0}}";

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDate.Value.Year.Should().Be(model.LocalDate.Value.Year);
            result.LocalDate.Value.Month.Should().Be(model.LocalDate.Value.Month);
            result.LocalDate.Value.Day.Should().Be(model.LocalDate.Value.Day);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaDate_WorksWithOldLocalDateFormatWithoutTypeMap()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDate = new LocalDate(2025, 12, 2);
            var json = "{\"$type\":\"UBind.Domain.Tests.Events.NodaTimeExampleModel, UBind.Domain.Tests\"," +
            "\"localDate\":{\r\n\t\t\t\"year\": " + model.LocalDate.Value.Year + ",\r\n\t\t\t\"month\": " + model.LocalDate.Value.Month + ",\r\n\t\t\t\"day\": " + model.LocalDate.Value.Day + ",\r\n\t\t\t\"calendar\":0\r\n\t\t}}";
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            setting.Converters.Add(new NodaLocalDateConverter());

            // Act
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDate.Value.Year.Should().Be(model.LocalDate.Value.Year);
            result.LocalDate.Value.Month.Should().Be(model.LocalDate.Value.Month);
            result.LocalDate.Value.Day.Should().Be(model.LocalDate.Value.Day);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaDate_WorksWithNewLocalDateFormat()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDate = new LocalDate(2025, 12, 2);
            var json = JsonConvert.SerializeObject(model, CustomSerializerSetting.JsonSerializerSettings);
            var setting = CustomSerializerSetting.JsonSerializerSettings;

            // Act
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDate.Value.Year.Should().Be(model.LocalDate.Value.Year);
            result.LocalDate.Value.Month.Should().Be(model.LocalDate.Value.Month);
            result.LocalDate.Value.Day.Should().Be(model.LocalDate.Value.Day);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaDateTime_WorksWithOldLocalDateTimeFormat()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDateTime = new LocalDateTime(2025, 12, 2, 1, 1, 1);
            var json = "{\"$type\":\"UBind.Domain.Tests.Events.NodaTimeExampleModel, UBind.Domain.Tests\"," +
                       "\"localDateTime\":{\"$type\":\"NodaTime.LocalDate, NodaTime\"," +
                       "\"year\":" + model.LocalDateTime.Value.Year + ",\"month\":" + model.LocalDateTime.Value.Month +
                       ",\"day\":" + model.LocalDateTime.Value.Day + ",\"calendar\":0}}";

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDateTime.Value.Year.Should().Be(model.LocalDateTime.Value.Year);
            result.LocalDateTime.Value.Month.Should().Be(model.LocalDateTime.Value.Month);
            result.LocalDateTime.Value.Day.Should().Be(model.LocalDateTime.Value.Day);
            result.LocalDateTime.Value.Hour.Should().Be(0);
            result.LocalDateTime.Value.Minute.Should().Be(0);
            result.LocalDateTime.Value.Second.Should().Be(0);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaDateTime_WorksWithOldLocalDateTimeWithNanoOfDayFormat()
        {
            long nanoOfDay = 57_600_000_000_000;

            int hours = (int)(nanoOfDay / 3_600_000_000_000L);
            int minutes = (int)((nanoOfDay % 3_600_000_000_000L) / 60_000_000_000L);
            int seconds = (int)((nanoOfDay % 60_000_000_000L) / 1_000_000_000L);
            int nanoseconds = (int)(nanoOfDay % 1_000_000_000L);

            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDateTime = new LocalDateTime(2025, 12, 2, hours, minutes, seconds).PlusNanoseconds(nanoseconds);
            var json = "{\"$type\":\"UBind.Domain.Tests.Events.NodaTimeExampleModel, UBind.Domain.Tests\"," +
                       "\"localDateTime\":{\"$type\":\"NodaTime.LocalDate, NodaTime\"," +
                       "\"year\":" + model.LocalDateTime.Value.Year + ",\"month\":" + model.LocalDateTime.Value.Month +
                       ",\"day\":" + model.LocalDateTime.Value.Day + ",\"calendar\":0, \"nanoOfDay\": " + nanoOfDay + "}}";

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDateTime.Value.Year.Should().Be(model.LocalDateTime.Value.Year);
            result.LocalDateTime.Value.Month.Should().Be(model.LocalDateTime.Value.Month);
            result.LocalDateTime.Value.Day.Should().Be(model.LocalDateTime.Value.Day);
            result.LocalDateTime.Value.Hour.Should().Be(model.LocalDateTime.Value.Hour);
            result.LocalDateTime.Value.Minute.Should().Be(model.LocalDateTime.Value.Minute);
            result.LocalDateTime.Value.Second.Should().Be(model.LocalDateTime.Value.Second);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaDateTime_WorksWithNewLocalDateTimeFormat()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.LocalDateTime = new LocalDateTime(2025, 12, 2, 1, 1, 1);
            var json = JsonConvert.SerializeObject(model, CustomSerializerSetting.JsonSerializerSettings);

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Should().NotBeNull();
            result.LocalDateTime.Value.Year.Should().Be(model.LocalDateTime.Value.Year);
            result.LocalDateTime.Value.Month.Should().Be(model.LocalDateTime.Value.Month);
            result.LocalDateTime.Value.Day.Should().Be(model.LocalDateTime.Value.Day);
            result.LocalDateTime.Value.Hour.Should().Be(model.LocalDateTime.Value.Hour);
            result.LocalDateTime.Value.Minute.Should().Be(model.LocalDateTime.Value.Minute);
            result.LocalDateTime.Value.Second.Should().Be(model.LocalDateTime.Value.Second);
        }

        [Fact]
        public void CustomSerializerSetting_serializeInstant_WorksWithOldLocalInstantFormat()
        {
            // Prepare
            IClock systemClock = SystemClock.Instance;
            Instant now = systemClock.Now();
            long days = now.ToUnixTimeTicks() / NodaConstants.TicksPerDay;
            long nanoOfDay = (now.ToUnixTimeTicks() % NodaConstants.TicksPerDay) * NodaConstants.NanosecondsPerTick;
            var model = new NodaTimeExampleModel();
            model.Instant = now;
            var json = "{\"$type\":\"UBind.Domain.Tests.Events.NodaTimeExampleModel, UBind.Domain.Tests\"," +
                       "\"instant\":{\"$type\":\"NodaTime.Instant, NodaTime\",\"days\":" + days + ",\"nanoOfDay\":" + nanoOfDay + "}}";

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Instant.Should().Be(model.Instant);
        }

        [Fact]
        public void CustomSerializerSetting_serializeLocaInstant_WorksWithNewLocalInstantFormat()
        {
            // Prepare
            var model = new NodaTimeExampleModel();
            model.Instant = new Instant();
            var json = JsonConvert.SerializeObject(model, CustomSerializerSetting.JsonSerializerSettings);

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<NodaTimeExampleModel>(json, setting);

            // Assert
            result.Instant.Should().Be(model.Instant);
        }

        [Fact]
        public void CustomSerializerSetting_ShouldDeserialize_AnObjectWithStringPropertyCorrectly()
        {
            // Prepare
            var jsonString = @"{""$type"":""UBind.Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueInitializedEvent`2[[UBind.Domain.Aggregates.Quote.QuoteAggregate, UBind.Domain],[UBind.Domain.Aggregates.Quote.IQuoteEventObserver, UBind.Domain]], UBind.Domain"",""AdditionalPropertyDefinitionId"":""93f9cdc9-e5f8-4739-b3ad-8e465ed827d7"",""EntityId"":""de08463f-b9e9-4d0e-ba77-903e1fef97e6"",""Value"":""2023-04-27"",""AdditionalPropertyDefinitionType"":0}";

            // Act
            var setting = CustomSerializerSetting.JsonSerializerSettings;
            var result = JsonConvert.DeserializeObject<AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver>>(jsonString, setting);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().NotBeNull();
            result.Value.Should().Be("2023-04-27");
        }
    }

    public class NodaTimeExampleModel
    {
        public LocalDate? LocalDate { get; set; }

        public LocalDateTime? LocalDateTime { get; set; }

        public Instant? Instant { get; set; }
    }
}
