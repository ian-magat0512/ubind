// <copyright file="EmailAddressField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// The field for email address.
    /// </summary>
    public class EmailAddressField : LabelledOrderedField
    {
        private EmailAddress emailAddressValueObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressField"/> class.
        /// </summary>
        /// <param name="label">The label of the email address.</param>
        /// <param name="customLabel">The custom label of the email address.</param>
        /// <param name="emailAddress">The email address value object.</param>
        public EmailAddressField(string label, string? customLabel, EmailAddress emailAddress)
            : base(label, customLabel)
        {
            this.emailAddressValueObject = emailAddress;
            this.EmailAddress = emailAddress.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressField"/> class.
        /// </summary>
        /// <param name="emailAddressReadModel">The email address read model.</param>
        public EmailAddressField(EmailAddressReadModel emailAddressReadModel)
            : base(
                  emailAddressReadModel.TenantId,
                  emailAddressReadModel.Id,
                  emailAddressReadModel.Label,
                  emailAddressReadModel.CustomLabel,
                  emailAddressReadModel.SequenceNo,
                  emailAddressReadModel.IsDefault)
        {
            this.Id = emailAddressReadModel.Id;
            if (!string.IsNullOrEmpty(emailAddressReadModel.EmailAddress))
            {
                this.emailAddressValueObject = new EmailAddress(emailAddressReadModel.EmailAddress);
                this.EmailAddress = emailAddressReadModel.EmailAddress;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressField"/> class.
        /// </summary>
        /// <remarks>
        /// Used for retrieving the aggregate events.
        /// </remarks>
        [JsonConstructor]
        public EmailAddressField()
        {
        }

        /// <summary>
        /// Gets or Sets the value of the email address.
        /// </summary>
        /// <remarks>
        /// This receives the value from the frontend.
        /// </remarks>
        [JsonProperty]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets the value of the email field.
        /// </summary>
        [JsonIgnore]
        public EmailAddress EmailAddressValueObject
        {
            get
            {
                if (this.emailAddressValueObject != null)
                {
                    this.EmailAddress = this.emailAddressValueObject.ToString();
                }
                else
                {
                    this.emailAddressValueObject
                        = !string.IsNullOrEmpty(this.EmailAddress) ? new EmailAddress(this.EmailAddress) : null;
                }

                return this.emailAddressValueObject;
            }
        }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "work", "personal", "other", };

        public bool IsValid()
        {
            Regex validation = new Regex("^(([^<>()[\\]\\\\.,;:\\s@\"]+(\\.[^<>()[\\]\\\\.,;:\\s@\"]+)*)|(\".+\"))@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\])|(([a-zA-Z\\-0-9]+\\.)+[a-zA-Z]{2,}))$");
            return validation.IsMatch(this.EmailAddress);
        }
    }
}
