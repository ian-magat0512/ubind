// <copyright file="IPersonReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Data transfer object for person read model summary.
    /// </summary>
    public interface IPersonReadModelSummary : IEntityReadModel<Guid>, IPersonalDetails
    {
        /// <summary>
        /// Gets or sets the customer Id.
        /// </summary>
        Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        Guid? UserId { get; set; }

        /// <summary>
        /// Gets or sets the account status of the person.
        /// </summary>
        [JsonProperty(PropertyName = "userStatus")]
        string Status { get; set; }

        /// <summary>
        /// Gets or sets the Owner ID.
        /// </summary>
        Guid? OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the fullname of the owner.
        /// </summary>
        string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has active policies.
        /// </summary>
        bool HasActivePolicies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been invited to activate.
        /// </summary>
        bool UserHasBeenInvitedToActivate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has been activated.
        /// </summary>
        bool UserHasBeenActivated { get; set; }
    }
}
