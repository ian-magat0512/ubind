// <copyright file="AuthenticationMethodReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Organisation;

using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

public class AuthenticationMethodReadModelSummary : EntityReadModel<Guid>, IAuthenticationMethodReadModelSummary
{
    /// <summary>
    /// Parameterless constructor for Entity Framework.
    /// </summary>
    public AuthenticationMethodReadModelSummary()
    {
    }

    protected AuthenticationMethodReadModelSummary(
        OrganisationAggregate.OrganisationAuthenticationMethodAddedEvent @event)
        : base(@event.TenantId, @event.AuthenticationMethod.Id, @event.Timestamp)
    {
        this.Update(@event);
    }

    public Guid OrganisationId { get; set; }

    public string Name { get; set; }

    public string TypeName { get; set; }

    public bool CanCustomersSignIn { get; set; }

    public bool CanAgentsSignIn { get; set; }

    public bool IncludeSignInButtonOnPortalLoginPage { get; set; }

    public string? SignInButtonBackgroundColor { get; set; }

    public string? SignInButtonIconUrl { get; set; }

    public string? SignInButtonLabel { get; set; }

    public bool Disabled { get; set; }

    public virtual void Update(OrganisationAggregate.OrganisationAuthenticationMethodUpsertEvent @event)
    {
        this.OrganisationId = @event.AggregateId;
        this.Name = @event.AuthenticationMethod.Name;
        this.CanCustomersSignIn = @event.AuthenticationMethod.CanCustomersSignIn;
        this.CanAgentsSignIn = @event.AuthenticationMethod.CanAgentsSignIn;
        this.IncludeSignInButtonOnPortalLoginPage = @event.AuthenticationMethod.IncludeSignInButtonOnPortalLoginPage;
        this.SignInButtonBackgroundColor = @event.AuthenticationMethod.SignInButtonBackgroundColor;
        this.SignInButtonIconUrl = @event.AuthenticationMethod.SignInButtonIconUrl;
        this.SignInButtonLabel = @event.AuthenticationMethod.SignInButtonLabel;
        this.Disabled = @event.AuthenticationMethod.Disabled;
    }
}
