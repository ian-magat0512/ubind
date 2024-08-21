// <copyright file="ClaimFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;

    /// <summary>
    /// Helper for generating claims for automated tests.
    /// </summary>
    public static class ClaimFactory
    {
        /// <summary>
        /// Create a claim for a given policy with some default data.
        /// </summary>
        /// <param name="quoteAggregate">A quote aggregate that has a policy.</param>
        /// <param name="clock">An optional clock, in case you want to fake the created time.</param>
        /// <returns>A new instance of <see cref="ClaimAggregate"/>.</returns>
        public static ClaimAggregate CreateClaim(QuoteAggregate quoteAggregate, IClock clock = null)
        {
            clock = clock ?? SystemClock.Instance;
            return ClaimAggregate.CreateForPolicy(
                "AAAAAA",
                quoteAggregate,
                Guid.NewGuid(),
                "John Smith",
                "Johnny",
                Guid.NewGuid(),
                clock.Now());
        }

        /// <summary>
        /// Add form data to a claim.
        /// </summary>
        /// <param name="claimAggregate">The claim.</param>
        /// <param name="formDataJson">The form data.</param>
        /// <param name="clock">An optional clock, in case you want to fake the event created time.</param>
        /// <returns>The claim for chaining fluent syntax.</returns>
        public static ClaimAggregate WithFormData(this ClaimAggregate claimAggregate, string formDataJson, IClock clock = null)
        {
            clock = clock ?? SystemClock.Instance;
            claimAggregate.UpdateFormData(formDataJson, Guid.NewGuid(), clock.Now());
            return claimAggregate;
        }

        /// <summary>
        /// Add form data to a claim, created from standard claims data.
        /// </summary>
        /// <param name="claimAggregate">The claim.</param>
        /// <param name="claimData">The claim data.</param>
        /// <param name="clock">An optional clock, in case you want to fake the event created time.</param>
        /// <returns>The claim for chaining fluent syntax.</returns>
        public static ClaimAggregate WithFormDatafromClaimData(this ClaimAggregate claimAggregate, IClaimData claimData, IClock clock = null)
        {
            var formModelObject = JObject.Parse(@"{ }");
            var claimDataMapper = new DefaultClaimDataMapper(new DefaultPolicyTransactionTimeOfDayScheme());
            var syncedFormModelObject = claimDataMapper.SyncFormdata(formModelObject, claimData);
            var formDataObject = JObject.Parse(@"{ }");
            formDataObject.Add("formModel", syncedFormModelObject);
            var formDataJson = formDataObject.ToString();
            return claimAggregate.WithFormData(formDataJson, clock);
        }
    }
}
