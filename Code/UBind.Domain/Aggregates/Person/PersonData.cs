// <copyright file="PersonData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Encapsulates person data for use in events.
    /// </summary>
    public class PersonData : PersonalDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonData"/> class with data from an instance of <see cref="PersonAggregate"/>.
        /// </summary>
        /// <param name="personAggregate">The instance of <see cref="PersonAggregate"/> to take the data from.</param>
        public PersonData(PersonAggregate personAggregate)
            : base(personAggregate)
        {
            this.PersonId = personAggregate.Id;
            this.TenantId = personAggregate.TenantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonData"/> class with data from an instance of <see cref="PersonAggregate"/>.
        /// </summary>
        /// <param name="personalDetails">The instance of <see cref="PersonAggregate"/> to take the data from.</param>
        /// <param name="personId">The person ID.</param>
        public PersonData(IPersonalDetails personalDetails, Guid personId)
            : base(personalDetails)
        {
            this.PersonId = personId;
            this.TenantId = personalDetails.TenantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonData"/> class.
        /// </summary>
        [JsonConstructor]
        public PersonData()
        {
        }

        /// <summary>
        /// Gets the ID of the person aggregate the data belongs to.
        /// </summary>
        [JsonProperty]
        public Guid PersonId { get; private set; }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// The Attribute is needed, because its not backward compatible for aggregate events.
        /// </summary>
        [JsonProperty("TenantNewId")]
        public override Guid TenantId { get; set; }
    }
}
