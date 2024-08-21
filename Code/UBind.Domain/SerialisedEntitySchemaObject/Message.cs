// <copyright file="Message.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for message type.
    /// </summary>
    public abstract class Message : BaseEntity<Domain.Entities.Message>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="id">The entity Id.</param>
        /// <param name="createdTicksSinceEpoch">The created ticks since epoch.</param>
        /// <param name="lastModifiedTicksSinceEpoch">The last modified ticks since epoch.</param>
        public Message(Guid id, long createdTicksSinceEpoch, long? lastModifiedTicksSinceEpoch)
            : base(id, createdTicksSinceEpoch, lastModifiedTicksSinceEpoch)
        {
            if (createdTicksSinceEpoch > 0)
            {
                var sentTimestampVariants = new DateTimeVariants(createdTicksSinceEpoch, Timezones.AET);
                this.SentTicksSinceEpoch = sentTimestampVariants.TicksSinceEpoch;
                this.SentDateTime = sentTimestampVariants.DateTime;
                this.SentDate = sentTimestampVariants.Date;
                this.SentTime = sentTimestampVariants.Time;
                this.SentTimeZoneName = sentTimestampVariants.TimeZoneName;
                this.SentTimeZoneAbbreviation = sentTimestampVariants.TimeZoneAbbreviation;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="id">The entity id.</param>
        public Message(Guid id)
            : base(id)
        {
        }

        protected Message()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the messge type.
        /// </summary>
        [JsonProperty(PropertyName = "type", Order = 0)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the sent ticks since epock .
        /// </summary>
        [JsonProperty(PropertyName = "sentTicksSinceEpoch", Order = 21)]
        public long? SentTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the date and time the email is sent.
        /// </summary>
        [JsonProperty(PropertyName = "sentDateTime", Order = 22)]
        [Required]
        public string SentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date the email is sent.
        /// </summary>
        [JsonProperty(PropertyName = "sentDate", Order = 23)]
        [Required]
        public string SentDate { get; set; }

        /// <summary>
        /// Gets or sets the time tthe email is sent.
        /// </summary>
        [JsonProperty(PropertyName = "sentTime", Order = 24)]
        [Required]
        public string SentTime { get; set; }

        /// <summary>
        /// Gets or sets the sent local time zone.
        /// </summary>
        [JsonProperty(PropertyName = "sentTimeZoneName", Order = 25)]
        [Required]
        public string SentTimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the sent local time zone alias.
        /// </summary>
        [JsonProperty(PropertyName = "sentTimeZoneAbbreviation", Order = 26)]
        [Required]
        public string SentTimeZoneAbbreviation { get; set; }
    }
}
