// <copyright file="TenantDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Tenant details.
    /// </summary>
    public class TenantDetails : Entity<Guid>
    {
        /// <summary>
        /// 30 days is the default fix length timeout in milliseconds.
        /// </summary>
        private const long DefaultFixLengthTimeoutMilliseconds = 2592000000;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <param name="name">The tenant name.</param>
        /// <param name="alias">The tenant name alias.</param>
        /// <param name="customDomain">The custom domain.</param>
        /// <param name="disabled">If tenant is disabled.</param>
        /// <param name="deleted">If tenant is deleted.</param>
        /// <param name="defaultOrganisationId">The Id of the organisation.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public TenantDetails(
            string name,
            string alias,
            string? customDomain,
            bool disabled,
            bool deleted,
            Guid defaultOrganisationId,
            Guid defaultPortalId,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = name;
            this.Alias = alias;
            this.CustomDomain = customDomain;
            this.Disabled = disabled;
            this.Deleted = deleted;
            this.DefaultOrganisationId = defaultOrganisationId;
            this.DefaultPortalId = defaultPortalId;
            this.IdleTimeoutPeriodType = string.Empty;
            this.IdleTimeoutInMilliseconds = 0;
            this.FixLengthTimeoutInPeriodType = string.Empty;
            this.FixLengthTimeoutInMilliseconds = 0;
            this.SessionExpiryMode = SessionExpiryMode.FixedPeriod;

            // default
            this.PasswordExpiryEnabled = false;
            this.MaxPasswordAgeDays = 0;
            this.FixLengthTimeoutInPeriodType = TokenSessionPeriodType.Day;
            this.FixLengthTimeoutInMilliseconds = DefaultFixLengthTimeoutMilliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class as a copy of existin tenant details.
        /// </summary>
        /// <param name="other">The details to copy.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public TenantDetails(TenantDetails other, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = other.Name;
            this.Alias = other.Alias;
            this.DefaultPortalStylesheetUrl = other.DefaultPortalStylesheetUrl;
            this.CustomDomain = other.CustomDomain;
            this.DefaultPortalTitle = other.DefaultPortalTitle;
            this.Disabled = other.Disabled;
            this.Deleted = other.Deleted;
            this.DefaultOrganisationId = other.DefaultOrganisationId;
            this.DefaultPortalId = other.DefaultPortalId;
            this.IdleTimeoutInMilliseconds = other.IdleTimeoutInMilliseconds;
            this.FixLengthTimeoutInMilliseconds = other.FixLengthTimeoutInMilliseconds;
            this.IdleTimeoutPeriodType = other.IdleTimeoutPeriodType;
            this.FixLengthTimeoutInPeriodType = other.FixLengthTimeoutInPeriodType;
            this.SessionExpiryMode = other.SessionExpiryMode;
            this.PasswordExpiryEnabled = other.PasswordExpiryEnabled;
            this.MaxPasswordAgeDays = other.MaxPasswordAgeDays;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <param name="sessionExpiryMode">The session expiry mode.</param>
        /// <param name="idleTimeoutPeriodType">idle timeout period type.</param>
        /// <param name="idleTimeout">the idle timeout by period.</param>
        /// <param name="fixLengthTimeoutInPeriodType">fix length timeout period type.</param>
        /// <param name="fixLengthTimeout">fix length timeout by period.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public TenantDetails(
            SessionExpiryMode sessionExpiryMode,
            string idleTimeoutPeriodType,
            long idleTimeout,
            string fixLengthTimeoutInPeriodType,
            long fixLengthTimeout,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.SessionExpiryMode = sessionExpiryMode;
            this.IdleTimeoutPeriodType = idleTimeoutPeriodType;
            this.IdleTimeoutInMilliseconds = this.TimeoutToMilliseconds(idleTimeoutPeriodType, idleTimeout);
            this.FixLengthTimeoutInPeriodType = fixLengthTimeoutInPeriodType;
            this.FixLengthTimeoutInMilliseconds = this.TimeoutToMilliseconds(fixLengthTimeoutInPeriodType, fixLengthTimeout);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private TenantDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the tenant name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the tenant name alias.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant was disabled.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets the portal domain.
        /// </summary>
        public string? CustomDomain { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the tenant was deleted.
        /// </summary>
        public bool Deleted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the sesion should expiry from inactivity or after a fixed period.
        /// </summary>
        public SessionExpiryMode SessionExpiryMode { get; private set; }

        /// <summary>
        /// Gets the period type of the idle timeout value.
        /// </summary>
        public string IdleTimeoutPeriodType { get; private set; }

        /// <summary>
        /// Gets the period type of the fix length timeout value.
        /// </summary>
        public string FixLengthTimeoutInPeriodType { get; private set; }

        /// <summary>
        /// Gets the idle timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        public long IdleTimeoutInMilliseconds { get; private set; }

        /// <summary>
        /// Gets the fix length timeout in milliseconds for the specific tenant for security purposes.
        /// </summary>
        public long FixLengthTimeoutInMilliseconds { get; private set; }

        /// <summary>
        /// Gets the default organisation of the tenancy.
        /// </summary>
        public Guid DefaultOrganisationId { get; private set; }

        public Guid DefaultPortalId { get; set; }

        /// <summary>
        /// Gets the url to a stylesheet which is loaded when rendering the default portal for this tenant.
        /// The stylesheet or css file as an external link.
        /// </summary>
        public string DefaultPortalStylesheetUrl { get; private set; }

        /// <summary>
        /// Gets the title to display for the default portal.
        /// </summary>
        public string DefaultPortalTitle { get; private set; }

        /// <summary>
        /// Gets a value indicating whether gets the password expiry is enabled.
        /// </summary>
        public bool PasswordExpiryEnabled { get; private set; }

        /// <summary>
        /// Gets the max password age days.
        /// </summary>
        public decimal MaxPasswordAgeDays { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <param name="name">The tenant name.</param>
        /// <param name="alias">The tenant name alias.</param>
        /// <param name="customDomain">The custom domain.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <returns>the tenant details.</returns>
        public TenantDetails UpdateDetails(
            string name,
            string alias,
            string customDomain,
            bool disabled,
            bool deleted,
            Guid organisationId)
        {
            this.Name = name;
            this.Alias = alias;
            this.CustomDomain = customDomain;
            this.DefaultOrganisationId = organisationId;
            this.Disabled = disabled;
            this.Deleted = deleted;
            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <param name="sessionExpiryMode">The session expiry mode.</param>
        /// <param name="idleTimeOutPeriodType">The tenant name.</param>
        /// <param name="idleTimeOutInMilliseconds">The tenant name alias.</param>
        /// <param name="fixLengthTimeoutInPeriodType">If tenant is disabled.</param>
        /// <param name="fixLengthTimeoutInMilliseconds">If tenant is deleted.</param>
        /// <returns>the tenant details.</returns>
        public TenantDetails UpdateSession(
            SessionExpiryMode sessionExpiryMode,
            string idleTimeOutPeriodType,
            long idleTimeOutInMilliseconds,
            string fixLengthTimeoutInPeriodType,
            long fixLengthTimeoutInMilliseconds)
        {
            this.SessionExpiryMode = sessionExpiryMode;
            this.IdleTimeoutPeriodType = idleTimeOutPeriodType;
            this.IdleTimeoutInMilliseconds = idleTimeOutInMilliseconds;
            this.FixLengthTimeoutInPeriodType = fixLengthTimeoutInPeriodType;
            this.FixLengthTimeoutInMilliseconds = fixLengthTimeoutInMilliseconds;

            return this;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDetails"/> class.
        /// </summary>
        /// <param name="passwordExpiryEnabled">The password expiry is enabled?.</param>
        /// <param name="maxPasswordAgeDays">The max password age days.</param>
        /// <returns>the tenant details.</returns>
        public TenantDetails UpdatePasswordExpiry(
            bool passwordExpiryEnabled,
            decimal maxPasswordAgeDays)
        {
            this.PasswordExpiryEnabled = passwordExpiryEnabled;
            this.MaxPasswordAgeDays = maxPasswordAgeDays;

            return this;
        }

        /// <summary>
        /// Updates the default organisation reference Id.
        /// </summary>
        /// <param name="organisationId">The new organisation Id.</param>
        /// <returns>Updated instance of the tenant details.</returns>
        public TenantDetails UpdateDefaultOrganisation(Guid organisationId)
        {
            this.DefaultOrganisationId = organisationId;
            return this;
        }

        /// <summary>
        /// converts idle timeout to its period equivalent.
        /// </summary>
        /// <returns>the value.</returns>
        public long IdleTimeoutByPeriod()
        {
            switch (this.IdleTimeoutPeriodType)
            {
                case TokenSessionPeriodType.Day:
                    return this.IdleTimeoutInMilliseconds / 86400000;
                case TokenSessionPeriodType.Hour:
                    return this.IdleTimeoutInMilliseconds / 3600000;
                case TokenSessionPeriodType.Minute:
                    return this.IdleTimeoutInMilliseconds / 60000;
                default:
                    break;
            }

            return 0;
        }

        /// <summary>
        /// converts fix length timeout to its period equivalent.
        /// </summary>
        /// <returns>the value.</returns>
        public long FixLengthTimeoutByPeriod()
        {
            switch (this.FixLengthTimeoutInPeriodType)
            {
                default:
                case TokenSessionPeriodType.Day:
                    return this.FixLengthTimeoutInMilliseconds / 86400000;
                case TokenSessionPeriodType.Hour:
                    return this.FixLengthTimeoutInMilliseconds / 3600000;
                case TokenSessionPeriodType.Minute:
                    return this.FixLengthTimeoutInMilliseconds / 60000;
            }
        }

        /// <summary>
        /// Updates the name to a new value.
        /// </summary>
        /// <param name="name">The name.</param>
        public void UpdateName(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Updates the alias to a new value.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public void UpdateAlias(string alias)
        {
            this.Alias = alias;
        }

        /// <summary>
        /// turns timeout into milliseconds.
        /// </summary>
        /// <param name="period">period type.</param>
        /// <param name="idleTimeOut">idle timeout.</param>
        /// <returns>milliseconds.</returns>
        private long TimeoutToMilliseconds(string period, long idleTimeOut)
        {
            switch (period)
            {
                case TokenSessionPeriodType.Day:
                    return idleTimeOut * 86400000;
                case TokenSessionPeriodType.Hour:
                    return idleTimeOut * 3600000;
                case TokenSessionPeriodType.Minute:
                    return idleTimeOut * 60000;
                default:
                    return 0;
            }
        }
    }
}
