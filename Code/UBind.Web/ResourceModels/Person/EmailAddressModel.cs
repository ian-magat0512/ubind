// <copyright file="EmailAddressModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Resource model for person's email address.
    /// </summary>
    public class EmailAddressModel : FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressModel"/> class.
        /// </summary>
        /// <param name="emailAddressField">The email address field.</param>
        public EmailAddressModel(EmailAddressField emailAddressField)
            : base(emailAddressField.Id, emailAddressField.Label, emailAddressField.CustomLabel)
        {
            this.SequenceNo = emailAddressField.SequenceNo;
            this.IsDefault = emailAddressField.IsDefault;
            this.EmailAddress = emailAddressField.EmailAddressValueObject.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for email address model.
        /// </remarks>
        protected EmailAddressModel()
        {
        }

        /// <summary>
        /// Gets the email address.
        /// </summary>
        public string EmailAddress { get; private set; }
    }
}
