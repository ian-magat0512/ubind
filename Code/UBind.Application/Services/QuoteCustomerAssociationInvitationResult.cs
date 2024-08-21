// <copyright file="QuoteCustomerAssociationInvitationResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// Result class from quote and customer association.
    /// </summary>
    public class QuoteCustomerAssociationInvitationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCustomerAssociationInvitationResult"/> class.
        /// </summary>
        /// <param name="invitationId">The ID of the association invitation.</param>
        /// <param name="userAggregate">Instance of the customer user aggregate.</param>
        /// <param name="personAggregate">Instance of the customer person aggregate.</param>
        public QuoteCustomerAssociationInvitationResult(
            Guid invitationId, UserAggregate userAggregate, PersonAggregate personAggregate)
        {
            this.InvitationId = invitationId;
            this.UserAggregate = userAggregate;
            this.PersonAggregate = personAggregate;
        }

        /// <summary>
        /// Gets the ID of the association invitation.
        /// </summary>
        public Guid InvitationId { get; private set; }

        /// <summary>
        /// Gets the user aggregate.
        /// </summary>
        public UserAggregate UserAggregate { get; private set; }

        /// <summary>
        /// Gets the person aggregate.
        /// </summary>
        public PersonAggregate PersonAggregate { get; private set; }
    }
}
