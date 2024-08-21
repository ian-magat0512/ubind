// <copyright file="Tenant.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// A uBind tenant.
    /// </summary>
    public class Tenant : Entity<Guid>
    {
        /// <summary>
        /// Initializes the static properties.
        /// </summary>
        static Tenant()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="id">The identifier for tenant.</param>
        /// <param name="name">A descriptive name for the tenant.</param>
        /// <param name="alias">A descriptive alias for the tenant.</param>
        /// <param name="customDomain">The custom domain.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        public Tenant(
            Guid id,
            string name,
            string alias,
            string? customDomain,
            Guid defaultOrganisationId,
            Guid defaultPortalId,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            var details = new TenantDetails(
                name,
                alias,
                customDomain,
                false,
                false,
                defaultOrganisationId,
                defaultPortalId,
                createdTimestamp);

            this.DetailsCollection.Add(details);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <param name="tenantId">the existing tenant id value.</param>
        public Tenant(Guid tenantId)
            : base(tenantId, default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tenant"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private Tenant()
        {
        }

        /// <summary>
        /// Gets the Id of the master tenant.
        /// </summary>
        public static string MasterTenantAlias => "ubind"; // TODO: Change this to "master"

        /// <summary>
        /// Gets the Id of the master tenant.
        /// </summary>
        public static Guid MasterTenantId => Guid.Parse("fba5a5f0-17ca-4f29-9f52-2c4aabb26c4d");

        public static string MasterTenantName => "Master";

        public bool IsMasterTenant => this.Id == MasterTenantId;

        /// <summary>
        /// Gets the tenant details.
        /// </summary>
        public TenantDetails Details => this.History.FirstOrDefault();

        /// <summary>
        /// Gets all the details versions with most recent first.
        /// </summary>
        public IEnumerable<TenantDetails> History
        {
            get
            {
                return this.DetailsCollection.OrderByDescending(d => d.CreatedTimestamp);
            }
        }

        /// <summary>
        /// Gets or sets historic tenant details.
        /// </summary>
        /// <remarks>
        /// Required for EF to persist all historic and current details (unordered).
        /// .</remarks>
        public Collection<TenantDetails> DetailsCollection { get; set; }
            = new Collection<TenantDetails>();

        /// <summary>
        /// Throws an exception if the tenant is null.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="tenant">The tenant.</param>
        public static void ThrowIfNotFound(Guid tenantId, Tenant tenant)
        {
            if (tenant == null)
            {
                throw new ErrorException(Errors.Organisation.TenantNotFound(tenantId));
            }
        }

        /// <summary>
        /// Update the tenant with new details.
        /// </summary>
        /// <param name="details">The new tenant details.</param>
        public void Update(TenantDetails details)
        {
            this.DetailsCollection.Add(details);
        }

        /// <summary>
        /// Sets the default organisation reference for the tenant.
        /// </summary>
        /// <remarks>
        /// Creates a new tenant details, copying the previous information with the new default organisation Id.
        /// Any modification to the organisation Id will create a new details entry and replace the previous version.
        /// </remarks>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="timestamp">The time of the update as an Instant.</param>
        public void SetDefaultOrganisation(Guid organisationId, Instant timestamp)
        {
            var newTenantDetails = new TenantDetails(this.Details, timestamp);
            newTenantDetails.UpdateDefaultOrganisation(organisationId);
            this.Update(newTenantDetails);
        }

        /// <summary>
        /// Gets the first ever alias set of this tenant.
        /// This is useful for backward compatibility because before we have string Ids and some events use this.
        /// </summary>
        /// <returns>The initial alias.</returns>
        public string GetStringId()
        {
            return this.History.Last().Alias;
        }
    }
}
