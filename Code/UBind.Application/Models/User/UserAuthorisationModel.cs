// <copyright file="UserAuthorisationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.User
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Data returned/fetched after a successful login which contains information about the user
    /// and what they are authorised to access.
    /// </summary>
    public class UserAuthorisationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAuthorisationModel"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="accessToken">An access token.</param>
        /// <param name="tenantAlias">The user's tenant alias.</param>
        /// <param name="organisationAlias">The user's organisation alias.</param>
        /// <param name="portalId">The portal ID to load if the portal that they logged into cannot be used.
        /// This is useful when a customer logs into an agent portal.</param>
        /// <param name="portalOrganisationId">The ID of the organisation that the portal belongs to.</param>
        /// <param name="portalOrganisationAlias">The alias of the organisation that the portal belongs to.</param>
        public UserAuthorisationModel(
            UserReadModel user,
            string? accessToken,
            string tenantAlias,
            string organisationAlias,
            List<Permission> permissions,
            Guid? portalId,
            Guid? portalOrganisationId,
            string? portalOrganisationAlias,
            Guid? authenticationMethodId,
            AuthenticationMethodType? authenticationMethodType,
            bool? supportsSingleLogout = false)
        {
            this.UserId = user.Id;
            this.CustomerId = user.CustomerId;
            this.EmailAddress = user.Email;
            this.FullName = user.FullName;
            this.PreferredName = user.PreferredName;
            this.UserType = user.UserType;
            this.AccessToken = accessToken;
            this.TenantAlias = tenantAlias;
            this.Environment = user.Environment;
            this.TenantId = user.TenantId;
            this.OrganisationId = user.OrganisationId;
            this.OrganisationAlias = organisationAlias;
            this.PortalId = portalId;
            this.PortalOrganisationId = portalOrganisationId;
            this.PortalOrganisationAlias = portalOrganisationAlias;
            this.ProfilePictureId = user.ProfilePictureId?.ToString();

            if (string.IsNullOrEmpty(this.PreferredName))
            {
                this.PreferredName = user.FirstName;
            }

            this.Permissions = permissions.Select(p => p.Humanize().Camelize());
            this.AuthenticationMethodId = authenticationMethodId;
            this.AuthenticationMethodType = authenticationMethodType?.Humanize();
            this.SupportsSingleLogout = supportsSingleLogout;
        }

        /// <summary>
        /// Gets the deployment environment the user belogs to, or null if the user has access to all environments.
        /// </summary>
        [JsonProperty]
        public DeploymentEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        [JsonProperty]
        public string? AccessToken { get; private set; }

        /// <summary>
        /// Gets the user's ID.
        /// </summary>
        [JsonProperty]
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        [JsonProperty]
        public string EmailAddress { get; private set; }

        /// <summary>
        /// Gets the user's full name.
        /// </summary>
        [JsonProperty]
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the user's preferred name.
        /// </summary>
        [JsonProperty]
        public string PreferredName { get; private set; }

        /// <summary>
        /// Gets the user's role.
        /// </summary>
        [JsonProperty]
        public string UserType { get; private set; }

        /// <summary>
        /// Gets the customer Id of the user, if any, otherwise default.
        /// </summary>
        [JsonProperty]
        public Guid? CustomerId { get; private set; }

        /// <summary>
        /// Gets the user's tenant's alias.
        /// </summary>
        [JsonProperty]
        public string TenantAlias { get; private set; }

        /// <summary>
        /// Gets the user's string tenant id.
        /// </summary>
        [JsonProperty]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the user's organisation id.
        /// </summary>
        [JsonProperty]
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the user's organisation alias.
        /// </summary>
        [JsonProperty]
        public string OrganisationAlias { get; private set; }

        /// <summary>
        /// Gets the organisation id associated with the user's portal.
        /// </summary>
        [JsonProperty]
        public Guid? PortalOrganisationId { get; private set; }

        /// <summary>
        /// Gets the organisation alias associated with the user's portal.
        /// </summary>
        [JsonProperty]
        public string? PortalOrganisationAlias { get; private set; }

        /// <summary>
        /// Gets the ID of the users portal, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public Guid? PortalId { get; private set; }

        /// <summary>
        /// Gets the user's profile picture id, if any, otherwise null.
        /// </summary>
        [JsonProperty]
        public string? ProfilePictureId { get; }

        [JsonProperty]
        public IEnumerable<string> Permissions { get; }

        [JsonProperty]
        public Guid? AuthenticationMethodId { get; }

        [JsonProperty]
        public string? AuthenticationMethodType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's authentication method supports single logout.
        /// This typically used by SAML authentication methods, so that when logging out, we can log them
        /// out at the Identity provider, which also logs them out of other applications that use the same
        /// Idp session.
        /// </summary>
        [JsonProperty]
        public bool? SupportsSingleLogout { get; set; }
    }
}
