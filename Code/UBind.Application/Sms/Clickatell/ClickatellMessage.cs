// <copyright file="ClickatellMessage.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Sms.Clickatell
{
    using UBind.Domain.ValueTypes;

    public class ClickatellMessage
    {
        private const string CountryCode = "AU";

        public ClickatellMessage(PhoneNumber to, string content)
        {
            this.To = this.GenerateE164Format(to);
            this.Content = content;
        }

        public string Channel { get; private set; } = "sms";

        public string Content { get; private set; }

        public string To { get; private set; }

        /// <summary>
        /// Generate a E.164 format number (+61491234567).
        /// </summary>
        /// <param name="number">The phone number.</param>
        /// <returns>The formatted string.</returns>
        private string GenerateE164Format(PhoneNumber number)
        {
            var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();

            var phoneNumber = number.ToString();
            phoneNumber = phoneNumber.StartsWith("0") || phoneNumber.StartsWith("+") ? phoneNumber : "+" + phoneNumber;

            // if the number has no country code, it will use AU code (+61)
            var parsedNumber = phoneNumberUtil.Parse(phoneNumber, CountryCode);
            var formatNumber = phoneNumberUtil.Format(parsedNumber, PhoneNumbers.PhoneNumberFormat.E164);
            return formatNumber;
        }
    }
}
