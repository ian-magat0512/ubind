// <copyright file="Event.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Automation
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Attributes;
    using UBind.Domain.Events;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// Representation of an event for use in automations.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets or sets the ID of the event.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the created time of the event as the number of ticks since the epoch.
        /// </summary>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the created time of the event.
        /// </summary>
        [DatabaseProperty(nameof(CreatedTicksSinceEpoch))]
        public string CreatedDateTime => new DateTimeVariants(this.CreatedTicksSinceEpoch, Timezones.AET).DateTime;

        /// <summary>
        /// Gets the date the event was created in AET time zone.
        /// </summary>
        public string CreatedDate => new DateTimeVariants(this.CreatedTicksSinceEpoch, Timezones.AET).Date;

        /// <summary>
        /// Gets the time the event was created in the AET time zone.
        /// </summary>
        public string CreatedTime => new DateTimeVariants(this.CreatedTicksSinceEpoch, Timezones.AET).Time;

        /// <summary>
        /// Gets the time zone used for calculating the the local created date and time.
        /// </summary>
        /// <remarks>In future we may use different time zones, but for now this is fixed to AET.</remarks>
        public string CreatedTimeZoneName => new DateTimeVariants(this.CreatedTicksSinceEpoch, Timezones.AET).Time;

        /// <summary>
        /// Gets the alias of the time zone used for calculating the local created date and time.
        /// </summary>
        /// <remarks>In future we may use different time zones, but for now this is fixed to AET.</remarks>
        public string CreatedTimeZoneAbbreviation => new DateTimeVariants(this.CreatedTicksSinceEpoch, Timezones.AET).TimeZoneAbbreviation;

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public SystemEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the custom alias of the event, if set, otherwise null.
        /// </summary>
        public string CustomEventAlias { get; set; }

        /// <summary>
        /// Gets or sets the event's tags.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
    }
}
