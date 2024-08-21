// <copyright file="PortalEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Persistence.ReadModels.Portal;

    /// <summary>
    /// Aggregator for multiple dispatch of events from user aggregates.
    /// </summary>
    public class PortalEventAggregator : EventAggregator<PortalAggregate, Guid>, IPortalEventObserver
    {
        public PortalEventAggregator(
            IPortalReadModelWriter portalReadModelWriter,
            IPortalSignInMethodReadModelWriter portalSignInMethodReadModelWriter,
            IPortalSystemEventEmitter portalSystemEventEmitter)
            : base(portalReadModelWriter, portalSignInMethodReadModelWriter, portalSystemEventEmitter)
        {
        }
    }
}
