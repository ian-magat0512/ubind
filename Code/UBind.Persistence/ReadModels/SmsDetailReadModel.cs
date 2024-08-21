// <copyright file="SmsDetailReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Sms details model.
    /// </summary>
    public class SmsDetailReadModel
    {
        /// <summary>
        /// Gets or sets the SMS.
        /// </summary>
        public Sms Sms { get; set; }

        /// <summary>
        /// Gets or sets Policy.
        /// </summary>
        public PolicyReadModel Policy { get; set; }

        /// <summary>
        /// Gets or sets the customer.
        /// </summary>
        public CustomerReadModel Customer { get; set; }

        /// <summary>
        /// Gets or sets the customer's primary person.
        /// </summary>
        public PersonReadModel CustomerPrimaryPerson { get; set; }

        /// <summary>
        /// Gets or sets the quote.
        /// </summary>
        public NewQuoteReadModel Quote { get; set; }

        /// <summary>
        /// Gets or sets the claim.
        /// </summary>
        public ClaimReadModel Claim { get; set; }

        /// <summary>
        /// Gets or sets the policy transaction.
        /// </summary>
        public PolicyTransaction PolicyTransaction { get; set; }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public UserReadModel User { get; internal set; }

        /// <summary>
        /// Gets the organisation source of the Sms.
        /// </summary>
        public OrganisationReadModel Organisation { get; internal set; }
    }
}
