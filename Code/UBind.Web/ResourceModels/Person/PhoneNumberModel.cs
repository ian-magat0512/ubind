// <copyright file="PhoneNumberModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Person
{
    using UBind.Domain.Aggregates.Person.Fields;

    /// <summary>
    /// Resource model for person's phone number.
    /// </summary>
    public class PhoneNumberModel : FieldResourceModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberModel"/> class.
        /// </summary>
        /// <param name="phoneNumberField">The phone number field.</param>
        public PhoneNumberModel(PhoneNumberField phoneNumberField)
            : base(phoneNumberField.Id, phoneNumberField.Label, phoneNumberField.CustomLabel)
        {
            this.SequenceNo = phoneNumberField.SequenceNo;
            this.IsDefault = phoneNumberField.IsDefault;
            this.PhoneNumber = phoneNumberField.PhoneNumberValueObject?.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberModel"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for phone number model.
        /// </remarks>
        protected PhoneNumberModel()
        {
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        public string PhoneNumber { get; private set; }
    }
}
