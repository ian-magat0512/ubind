﻿// <copyright file="EmailAddressReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Person.Fields
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// The read model for email address.
    /// </summary>
    public class EmailAddressReadModel : LabelledOrderedField, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="emailAddressField">The email address field value object.</param>
        public EmailAddressReadModel(Guid tenantId, EmailAddressField emailAddressField)
            : base(
                  tenantId,
                  Guid.NewGuid(),
                  emailAddressField.Label,
                  emailAddressField.CustomLabel,
                  emailAddressField.SequenceNo,
                  emailAddressField.IsDefault)
        {
            this.EmailAddress = emailAddressField.EmailAddressValueObject?.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressReadModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        protected EmailAddressReadModel()
        {
        }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        [JsonProperty("emailAddress")]
        public string EmailAddress { get; private set; }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "work", "personal", "other", };
    }
}
