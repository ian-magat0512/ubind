// <copyright file="PeriodicTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    public class PeriodicTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public PeriodicTriggerConfigModel(
            string name,
            string alias,
            string description,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IBuilder<IProvider<Data<string>>> context)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.Context = context;
        }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty(Required = Required.Always)]
        public string Alias { get; private set; }

        [JsonProperty]
        public string Description { get; private set; }

        [JsonProperty]
        public IBuilder<IProvider<Data<bool>>> RunCondition { get; private set; }

        [JsonProperty]
        public string TimeZoneOffset { get; private set; }

        [JsonProperty]
        public string TimeZoneId { get; private set; }

        [JsonProperty(Required = Required.Always, ItemConverterType = typeof(TimeConfigModelConverter))]
        public TimeConfigModel Month { get; private set; }

        [JsonProperty(Required = Required.Always, ItemConverterType = typeof(TimeConfigModelConverter))]
        public DayConfigModel Day { get; private set; }

        [JsonProperty(Required = Required.Always, ItemConverterType = typeof(TimeConfigModelConverter))]
        public TimeConfigModel Hour { get; private set; }

        [JsonProperty(Required = Required.Always, ItemConverterType = typeof(TimeConfigModelConverter))]
        public TimeConfigModel Minute { get; private set; }

        [JsonProperty]
        public IBuilder<IProvider<Data<string>>> Context { get; private set; }

        public Trigger Build(IServiceProvider dependencyProvider)
        {
            var timeZoneInfo = TimeZoneInfo.Utc;
            var data = new JObject()
            {
                { ErrorDataKey.TriggerAlias, this.Alias },
                { ErrorDataKey.TriggerType, "periodicTrigger" },
            };

            if (!string.IsNullOrEmpty(this.TimeZoneOffset) &&
                !string.IsNullOrEmpty(this.TimeZoneId))
            {
                data.Add("TimeZoneOffset", this.TimeZoneOffset);
                data.Add("TimeZoneId", this.TimeZoneId);
                throw new ErrorException(Errors.Automation.AutomationConfigurationNotSupported(
                    $"Only one property is expected. Either {nameof(this.TimeZoneId)} or {nameof(this.TimeZoneOffset)} should be present in the configuration",
                    data));
            }

            if (!string.IsNullOrEmpty(this.TimeZoneOffset))
            {
                var regexForTimeZoneOffset = @"^[+-](?:2[0-3]|[01][0-9]):[0-5][0-9]$";
                var match = Regex.Match(this.TimeZoneOffset, regexForTimeZoneOffset, RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    data.Add("TimeZoneOffset", this.TimeZoneOffset);
                    data.Add("RequiredFormat", "+/-##:##");
                    throw new ErrorException(Errors.Automation.PropertyValueInvalid(nameof(this.TimeZoneOffset), this.Alias, data));
                }

                var offsets = this.TimeZoneOffset.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var offsetHours = Convert.ToInt32(offsets[0]);
                var offsetMinutes = Convert.ToInt32(offsets[1]);
                string displayName = $"(GMT{this.TimeZoneOffset}) Custom Time";
                string standardName = $"GMT{this.TimeZoneOffset}";
                var offset = new TimeSpan(offsetHours, offsetMinutes, 00);
                timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(standardName, offset, displayName, standardName);
                TimeZoneResolver.AddCustomTimeZone(timeZoneInfo);
            }

            if (!string.IsNullOrEmpty(this.TimeZoneId))
            {
                var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(this.TimeZoneId);
                if (zone == null)
                {
                    data.Add("TimeZoneId", this.TimeZoneId);
                    data.Add("SupportedTimeZones", "https://en.wikipedia.org/wiki/List_of_tz_database_time_zones");
                    throw new ErrorException(Errors.Automation.PropertyValueInvalid(nameof(this.TimeZoneId), this.Alias, data));
                }

                var timeZone = zone.ToTimeZoneInfo();
                if (zone == null)
                {
                    data.Add("TimeZoneId", this.TimeZoneId);
                    throw new ErrorException(Errors.General.Unexpected($"The time zone ID '{this.TimeZoneId}' was not found on the local computer."));
                }

                timeZoneInfo = timeZone;
            }

            return new PeriodicTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.RunCondition?.Build(dependencyProvider),
                timeZoneInfo,
                this.Month,
                this.Day,
                this.Hour,
                this.Minute);
        }
    }
}
