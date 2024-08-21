// <copyright file="StandardQuoteStates.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1623 // Property summary documentation should match accessors
namespace UBind.Domain.ValueTypes
{
    /// <summary>
    /// Represents the commonly known statuses a quote can be in.
    /// Note that a product may also be configured to allow quotes with custom states,
    /// so it's possible a quote's state may not be one of these.
    /// When comparing quote states, it's important to do a non case-sensitive comparison, e.g.:
    /// .EqualsIgnoreCase(StandardQuoteStates.Expired);.
    /// </summary>
    public static class StandardQuoteStates
    {
        /// <summary>
        /// Quotes which do not have enough information to be worth displaying in the portal are considered "Nascent".
        /// This typically means we don't have contact information abut the customer, but really, it's dependent upon
        /// when the "actualise" operation is run against a quote.
        /// </summary>
        public static string Nascent => "Nascent";

        /// <summary>
        /// Once a quote is "actualised" it becomes "Incomplete" and can now be viewed in the portal.
        /// </summary>
        public static string Incomplete => "Incomplete";

        /// <summary>
        /// The quote needs to be reviewed, e.g. by a broker.
        /// </summary>
        public static string Review => "Review";

        /// <summary>
        /// The quote needs to be endorsed, e.g. by an underwriter.
        /// </summary>
        public static string Endorsement => "Endorsement";

        /// <summary>
        /// The quote has been approved and can now be bound.
        /// </summary>
        public static string Approved => "Approved";

        /// <summary>
        /// The quote was declined and cannot be bound.
        /// </summary>
        public static string Declined => "Declined";

        /// <summary>
        /// The quote was completed.
        /// This usually means a policy was bound, unless the product was not configured to bind quotes.
        /// </summary>
        public static string Complete => "Complete";

        /// <summary>
        /// This quote expired.
        /// </summary>
        public static string Expired => "Expired";
    }
}
