// <copyright file="UserEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates
{
    using System;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.User;
    using UBind.Persistence;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.User;

    /// <summary>
    /// Aggregator for multiple dispatch of events from user aggregates.
    /// </summary>
    public class UserEventAggregator : EventAggregator<UserAggregate, Guid>, IUserEventObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserEventAggregator"/> class.
        /// </summary>> @event)
        /// <param name="userWriter">User writer for updating read model.</param>
        /// <param name="customerWriter">Writer for updating customer read models.</param>
        /// <param name="personReadModelWriter">Writer for the person read model.</param>
        public UserEventAggregator(
            IUserReadModelWriter userWriter,
            ICustomerReadModelWriter customerWriter,
            IPersonReadModelWriter personReadModelWriter,
            IUserLinkedIdentityReadModelWriter userLinkedIdentityReadModelWriter,
            IUserSystemEventEmitter userSystemEventEmitter)
            : base(userWriter,
                customerWriter,
                personReadModelWriter,
                userLinkedIdentityReadModelWriter,
                userSystemEventEmitter)
        {
        }
    }
}
