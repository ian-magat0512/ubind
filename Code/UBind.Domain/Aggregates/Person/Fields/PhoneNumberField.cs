// <copyright file="PhoneNumberField.cs" company="uBind">
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
    using LibPhoneNumber = PhoneNumbers;

    /// <summary>
    /// The phone number field.
    /// </summary>
    public class PhoneNumberField : LabelledOrderedField
    {
        private PhoneNumber phoneNumberValueObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberField"/> class.
        /// </summary>
        /// <param name="label">The label of the phone number.</param>
        /// <param name="customLabel">the custom label of the phone number.</param>
        /// <param name="phoneNumber">The phone number value object.</param>
        public PhoneNumberField(string label, string? customLabel, PhoneNumber phoneNumber)
            : base(label, customLabel)
        {
            this.phoneNumberValueObject = phoneNumber;
            this.PhoneNumber = phoneNumber.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberField"/> class.
        /// </summary>
        /// <param name="phoneNumberReadModel">The phone number read model.</param>
        public PhoneNumberField(PhoneNumberReadModel phoneNumberReadModel)
             : base(
                  phoneNumberReadModel.TenantId,
                  phoneNumberReadModel.Id,
                  phoneNumberReadModel.Label,
                  phoneNumberReadModel.CustomLabel,
                  phoneNumberReadModel.SequenceNo,
                  phoneNumberReadModel.IsDefault)
        {
            this.Id = phoneNumberReadModel.Id;
            if (!string.IsNullOrEmpty(phoneNumberReadModel.PhoneNumber))
            {
                this.phoneNumberValueObject = new PhoneNumber(phoneNumberReadModel.PhoneNumber);
                this.PhoneNumber = phoneNumberReadModel.PhoneNumber;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberField"/> class.
        /// </summary>
        /// <remarks>
        /// This is used when retrieving the aggregate event.
        /// </remarks>
        [JsonConstructor]
        public PhoneNumberField()
        {
        }

        /// <summary>
        ///  Gets or Sets the value of the phone number.
        /// </summary>
        /// <remarks>
        /// This receives the value from the frontend.
        /// </remarks>
        [JsonProperty]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets the value of the phone field.
        /// </summary>
        [JsonIgnore]
        public PhoneNumber PhoneNumberValueObject
        {
            get
            {
                if (this.phoneNumberValueObject != null)
                {
                    this.PhoneNumber = this.phoneNumberValueObject.ToString();
                }
                else
                {
                    this.phoneNumberValueObject = this.PhoneNumber != null ? new PhoneNumber(this.PhoneNumber) : null;
                }

                return this.phoneNumberValueObject;
            }
        }

        [JsonIgnore]
        protected override IEnumerable<string> LabelOptions => new List<string> { "mobile", "home", "work", "other", };

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is PhoneNumberField phoneNumber
                && this.PhoneNumberValueObject.Equals(phoneNumber.PhoneNumberValueObject)
                && this.Label.Equals(phoneNumber.Label);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool IsValid()
        {
            string cleanNumber = Regex.Replace(this.PhoneNumber, "/[\\ \\(\\)\\-]", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("+", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("-", string.Empty);
            Regex phoneNumberRegex = new Regex(@"^([\+]?61|0)[2|4|3|7|8][\d]{8}$");
            Regex phoneNumberRegex2 = new Regex(@"^1[3|8]00[\d]{6}$");
            Regex phoneNumberRegex3 = new Regex(@"^13[\d]{4}$");
            Regex phoneNumberRegex4 = new Regex(@"^([\+]?61|0)4[\d]{8}$");
            Regex phoneNumberRegex5 = new Regex(@"^\s*(0|\+\s*6\s*1)\s*(4|5)(\s*\d){8}\s*$");
            bool isValidForOtherCountry = this.IsPhoneNumberValidForOtherCountry();

            return
                (phoneNumberRegex.IsMatch(cleanNumber)
                || phoneNumberRegex2.IsMatch(cleanNumber)
                || phoneNumberRegex3.IsMatch(cleanNumber)
                || phoneNumberRegex4.IsMatch(cleanNumber)
                || phoneNumberRegex5.IsMatch(cleanNumber))
                || isValidForOtherCountry;
        }

        private bool IsPhoneNumberValidForOtherCountry()
        {
            LibPhoneNumber.PhoneNumberUtil? phoneNumberUtil = LibPhoneNumber.PhoneNumberUtil.GetInstance();
            try
            {
                LibPhoneNumber.PhoneNumber? phoneNumberProto = phoneNumberUtil.Parse(this.PhoneNumber, null);
                return phoneNumberUtil.GetRegionCodeForNumber(phoneNumberProto) != null;
            }
            catch (LibPhoneNumber.NumberParseException)
            {
                LibPhoneNumber.PhoneNumber? isAUValidPhoneNumber = phoneNumberUtil.Parse(this.PhoneNumber, "AU");
                return phoneNumberUtil.IsValidNumberForRegion(isAUValidPhoneNumber, "AU") ? true : false;
            }
        }
    }
}
