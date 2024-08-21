// <copyright file="OrganisationEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Persistence.ReadModels.Organisation;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

    /// <summary>
    /// Organisation event aggregator that passes organisation events to the organisation read model writers and other
    /// event observers.
    /// </summary>
    public class OrganisationEventAggregator : EventAggregator<OrganisationAggregate, Guid>, IOrganisationEventObserver
    {
        public OrganisationEventAggregator(
            IOrganisationReadModelWriter organisationReadModelWriter,
            IAuthenticationMethodReadModelWriter authenticationMethodReadModelWriter,
            IOrganisationSystemEventEmitter organisationSystemEventEmitter,
            IOrganisationLinkedIdentityReadModelWriter organisationLinkedIdentityReadModelWriter)
            : base(
                  organisationReadModelWriter,
                  authenticationMethodReadModelWriter,
                  organisationSystemEventEmitter,
                  organisationLinkedIdentityReadModelWriter)
        {
        }
    }
}
