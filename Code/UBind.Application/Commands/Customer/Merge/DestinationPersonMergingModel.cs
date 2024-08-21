// <copyright file="DestinationPersonMergingModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// The destination person param, with some more information needed for the merge to happen.
    /// </summary>
    internal class DestinationPersonMergingModel
    {
        public bool AssignToNewOwner;

        public DestinationPersonMergingModel(
            PersonAggregate personAggregate)
        {
            this.PersonalDetails = new PersonalDetails(personAggregate);
            this.PersonCustomerId = personAggregate.CustomerId.Value;
            this.PersonId = personAggregate.Id;
            this.AssignToNewOwner = false;
        }

        public PersonalDetails PersonalDetails { get; }

        public Guid PersonCustomerId { get; set; }

        public Guid PersonId { get; internal set; }

        /// <summary>
        /// The owner user Id. heavily relies on the AssignToNewOwner flag or else it will be null
        /// </summary>
        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// The owner person Id. heavily relies on the AssignToNewOwner flag or else it will be null
        /// </summary>
        public Guid? OwnerPersonId { get; private set; }

        /// <summary>
        /// You can specify the new owner here of the newly merged records.
        /// Uses the customer as the source.
        /// will be used on some entities like quotes or policies.
        /// </summary>
        public DestinationPersonMergingModel SetNewOwnerFromCustomer(CustomerReadModelDetail customer)
        {
            this.AssignToNewOwner = true;
            this.OwnerUserId = customer.OwnerUserId;
            this.OwnerPersonId = customer.OwnerPersonId;
            return this;
        }
    }
}
