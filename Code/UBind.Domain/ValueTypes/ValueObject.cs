// <copyright file="ValueObject.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base class for all value objects.
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// Method for comparing to other value object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if they are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            var other = (ValueObject)obj;

            return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// Method to override the default GetHashCode.
        /// </summary>
        /// <returns>The new hash code.</returns>
        public override int GetHashCode()
        {
            return this.GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// Method for checking equality of the two value objects.
        /// </summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if they are equal.</returns>
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            {
                return false;
            }

            return ReferenceEquals(left, null) || left.Equals(right);
        }

        /// <summary>
        /// Method for checking equality of the two value objects.
        /// </summary>
        /// <param name="left">The left object.</param>
        /// <param name="right">The right object.</param>
        /// <returns>True if they are not equal.</returns>
        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !EqualOperator(left, right);
        }

        /// <summary>
        /// Method for retrieving all equality components.
        /// </summary>
        /// <returns>The list of all equality components.</returns>
        protected abstract IEnumerable<object> GetEqualityComponents();
    }
}
