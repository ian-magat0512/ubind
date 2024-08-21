// <copyright file="CustomerSimpleModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;

    /// <summary>
    /// Resource model for serving the base identifying data of a customer.
    /// </summary>
    public class CustomerSimpleModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerSimpleModel"/> class.
        /// </summary>
        /// <param name="customerId">The id of the customer.</param>
        /// <param name="fullName">The full name of the customer.</param>
        /// <param name="isTestData">Whether this is test data.</param>
        public CustomerSimpleModel(Guid customerId, string fullName, bool isTestData = false)
        {
            this.Id = customerId;
            this.DisplayName = fullName;
            this.IsTestData = isTestData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerSimpleModel"/> class.
        /// </summary>
        /// <param name="customerId">The id of the customer.</param>
        /// <param name="fullName">The full name of the customer.</param>
        /// <param name="ownerUserId">The owner user id of the customer.</param>
        /// <param name="isTestData">Whether this is test data.</param>
        public CustomerSimpleModel(Guid customerId, string fullName, Guid? ownerUserId, bool isTestData = false)
        {
            this.Id = customerId;
            this.DisplayName = fullName;
            this.IsTestData = isTestData;
            this.OwnerUserId = ownerUserId;
        }

        /// <summary>
        /// Gets the ID of the customer record.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the display name of the customer.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the data is for test.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets the owner user id.
        /// </summary>
        public Guid? OwnerUserId { get; private set; }
    }
}
