// <copyright file="PersonEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using UBind.Domain.Aggregates.Person;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Quote;

    /// <summary>
    /// Aggregator for multiple dispatch of events from user aggregates.
    /// </summary>
    public class PersonEventAggregator : EventAggregator<PersonAggregate, Guid>, IPersonEventObserver
    {
        public PersonEventAggregator(
            IUserReadModelWriter userWriter,
            ICustomerReadModelWriter customerReadModelWriter,
            IQuoteReadModelWriter quoteReadModelWriter,
            IPolicyReadModelWriter policyReadModelWriter,
            IClaimReadModelWriter claimReadModelWriter,
            IPersonReadModelWriter personReadModelWriter)
            : base(
                  userWriter,
                  customerReadModelWriter,
                  quoteReadModelWriter,
                  policyReadModelWriter,
                  claimReadModelWriter,
                  personReadModelWriter)
        {
        }
    }
}
