// <copyright file="AutomationSystemData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Data
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain;

    /// <summary>
    /// Contains the data of the system that executes the current automation.
    /// </summary>
    public class AutomationSystemData
    {
        /*
        * By default, automation is expected to be always running in AET.
        * Ticket UB-10067 is created to make this configurable.
        */
        [JsonIgnore]
        public static DateTimeZone DefaultTimeZone = Timezones.AET;

        [JsonIgnore]
        public DateTimeZone TimeZone;

        private IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationSystemData"/> class.
        /// </summary>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="timeZone">The time zone offset to be used on output.</param>
        /// <param name="baseUrl">The base API url.</param>
        /// <param name="currentClock">The clock for checking time.</param>
        public AutomationSystemData(
            DeploymentEnvironment environment,
            string baseUrl,
            DateTimeZone timeZone,
            IClock currentClock)
        {
            this.Environment = environment;
            this.EnvironmentName = environment.ToString();
            this.BaseUrl = baseUrl;
            this.TimeZone = timeZone != null
                ? timeZone
                : AutomationSystemData.DefaultTimeZone;
            this.clock = currentClock;
        }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        [JsonProperty(PropertyName = "environment")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        [JsonProperty(PropertyName = "environmentName")]
        public string EnvironmentName { get; }

        /// <summary>
        /// Gets the base url of the application.
        /// </summary>
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the system time of the server as ticks since epoch.
        /// </summary>
        [JsonProperty("currentTicksSinceEpoch")]
        public long CurrentTicksSinceEpoch
        {
            get
            {
                return this.clock.GetCurrentInstant().ToUnixTimeTicks();
            }
        }

        /// <summary>
        /// Gets the system time of the server as an ISO date-time string in the configured offset.
        /// </summary>
        /// <remarks>
        /// Offset is currently always AET. This may be changed in future thus setting
        /// uses a passed timezone when converting.
        /// </remarks>
        [JsonProperty("currentDateTime")]
        public string CurrentDateTime
        {
            get
            {
                var now = this.clock.GetCurrentInstant();
                return OffsetDateTimePattern.ExtendedIso.Format(now.InZone(Timezones.AET).ToOffsetDateTime());
            }
        }

        public void SetProvider(IServiceProvider serviceProvider)
        {
            this.clock = serviceProvider.GetRequiredService<IClock>();
            this.TimeZone = Timezones.AET;
        }
    }
}
