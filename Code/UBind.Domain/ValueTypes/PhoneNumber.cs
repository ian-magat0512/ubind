// <copyright file="PhoneNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System.Collections.Generic;

    /// <summary>
    /// This class is needed to represent phone number as value object.
    /// </summary>
    public class PhoneNumber : ValueObject
    {
        private readonly string phoneNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> class.
        /// </summary>
        /// <param name="phoneNumber">The raw phone number.</param>
        public PhoneNumber(string phoneNumber)
        {
            this.phoneNumber = phoneNumber;
        }

        /// <summary>
        /// Method to override the ddefault ToString method.
        /// </summary>
        /// <returns>The phone number.</returns>
        public override string ToString()
        {
            return this.phoneNumber;
        }

        /// <summary>
        /// Method for overriding the GetEqualityComponents method.
        /// </summary>
        /// <returns>The list of equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.phoneNumber;
        }
    }
}
