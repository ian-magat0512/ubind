// <copyright file="LocalAccountAuthenticationMethodReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Organisation
{
    using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

    public class LocalAccountAuthenticationMethodReadModel : AuthenticationMethodReadModelSummary
    {
        public LocalAccountAuthenticationMethodReadModel(
            OrganisationAggregate.OrganisationAuthenticationMethodAddedEvent @event)
            : base(@event)
        {
            this.TypeName = "Local";
        }

        /// <summary>
        /// Parameterless constructor for Entity Framework.
        /// </summary>
        public LocalAccountAuthenticationMethodReadModel()
            : base()
        {
            this.TypeName = "Local";
        }

        public bool AllowCustomerSelfRegistration { get; set; }

        public bool AllowAgentSelfRegistration { get; set; }

        public override void Update(OrganisationAggregate.OrganisationAuthenticationMethodUpsertEvent @event)
        {
            base.Update(@event);
            LocalAccount source = (LocalAccount)@event.AuthenticationMethod;
            this.AllowCustomerSelfRegistration = source.AllowCustomerSelfRegistration;
            this.AllowAgentSelfRegistration = source.AllowAgentSelfRegistration;
        }
    }
}
