// <copyright file="EntityListFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents filters to apply when querying read models.
    /// </summary>
    public class EntityListFilters : IEntityListFilters
    {
        /// <summary>
        /// Gets or sets search keywords for filtering result sets.
        /// </summary>
        public IEnumerable<string> SearchTerms { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the name of the date field which DateIsAfterTicks and DateIsBeforeTicks relates to.
        /// </summary>
        public string? DateFilteringPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the time after which the object should have been filter by date in ticks since the epoch.
        /// </summary>
        public long? DateIsAfterTicks { get; set; }

        /// <summary>
        /// Gets or sets the time before which the object should have been filter by date in ticks since the epoch.
        /// </summary>
        public long? DateIsBeforeTicks { get; set; }

        /// <summary>
        /// Gets or sets status filters for filtering result sets.
        /// </summary>
        public IEnumerable<string> Statuses { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets statuses that will be excluded in filtering result sets.
        /// </summary>
        public IEnumerable<string> ExcludedStatuses { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets quote Type filters for filtering result sets.
        /// </summary>
        public IEnumerable<string> QuoteTypes { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets product Ids filters for filtering result sets.
        /// </summary>
        public IEnumerable<Guid> ProductIds { get; set; } = Enumerable.Empty<Guid>();

        /// <summary>
        /// Gets or sets email source filters for filtering result sets.
        /// </summary>
        public IEnumerable<string> Sources { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets a policyNumber for filtering related policy records.
        /// </summary>
        public string? PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets a policy ID for filtering related policy records.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets a policy transaction ID for filtering related policy transaction records.
        /// </summary>
        public Guid? PolicyTransactionId { get; set; }

        /// <summary>
        /// Gets or sets a quote ID for filtering related quote records.
        /// </summary>
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets a quote version ID for filtering related quote version records.
        /// </summary>
        public Guid? QuoteVersionId { get; set; }

        /// <summary>
        /// Gets or sets a claim ID for filtering related claim records.
        /// </summary>
        public Guid? ClaimId { get; set; }

        /// <summary>
        /// Gets or sets a claim version ID for filtering related claim records.
        /// </summary>
        public Guid? ClaimVersionId { get; set; }

        /// <summary>
        /// Gets or sets the Product Id to filter for.
        /// </summary>
        public Guid? ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Tenant Id to filter for.
        /// </summary>
        public Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is discarded or not.
        /// </summary>
        public bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets the Organisation Id to filter for.
        /// </summary>
        public IEnumerable<Guid>? OrganisationIds { get; set; }

        /// <summary>
        /// Gets or sets the Environment to filter for.
        /// </summary>
        public DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include data which is not environment specific.
        /// This can be used in situations where you have specified an environment (e.g. Development)
        /// to filter out Staging and Production Data, but you still want to include data
        /// which has no environment set (because it's not applicable).
        /// </summary>
        public bool IncludeNonEnvironmentSpecificData { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the user to filter on. This specifically filters on
        /// objects associated with a user account.
        /// </summary>
        public Guid? UserAccountId { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the owner to filter for.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer to filter against.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the data is for test.
        /// </summary>
        public bool IncludeTestData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include product feature.
        /// </summary>
        public bool IncludeProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        public int? Page { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the filter for any tags provided that can be related to any entity.
        /// </summary>
        public string[] Tags { get; set; } = new List<string>().ToArray();

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Gets or sets the entity in which the entity will be filtered by.
        /// </summary>
        public EntityType? EntityType { get; set; }

        /// <summary>
        /// Gets or sets the entity Id in which the entity will be filtered by.
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the order in which the selected entity field will be sorted.
        /// </summary>
        public SortDirection SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the portal alias the entity will be filtered by.
        /// </summary>
        public string? PortalAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is ride-protect.
        /// </summary>
        /// <remarks>
        /// This is a temporary hardcode condition for specific for ride-protect sub-organisation,
        /// to be removed after the implementation of UB-8372.
        /// </remarks>
        public bool IsRideProtectOrganisation { get; set; }

        /// <summary>
        /// Gets or sets ride protect product id.
        /// </summary>
        /// <remarks>
        /// This is a temporary hardcode condition for specific for ride-protect sub-organisation,
        /// to be removed after the implementation of UB-8372.
        /// </remarks>
        public Guid? RideProtectProductId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an email can be seen from other orgs.
        /// An exception to the rule.
        /// </summary>
        public bool CanViewEmailsFromOtherOrgThatHasCustomer { get; set; }

        /// <summary>
        /// Gets or sets the performing users organisation Id.
        /// </summary>
        public Guid? PerformingUserOrganisationId { get; set; }

        /// <summary>
        /// Sets a filter for the date after which the object should have been filter by date.
        /// </summary>
        /// <remarks>
        /// This is an exclusive limit. I.e. if the value is 1999-12-31, then only items created on or after 2000-01-01 will be included.
        /// </remarks>
        /// <param name="date">The date to use as the limit.</param>
        /// <param name="timezone">The timezone the date applies to. Defaults to AET if null or not specified.</param>
        /// <returns>The current instance, to support fluent syntax.</returns>
        public EntityListFilters WithDateIsAfterFilter(LocalDate date, DateTimeZone? timezone = null)
        {
            this.DateIsAfterTicks = date.GetTicksAtEndOfDayInZone(timezone ?? Timezones.AET);
            return this;
        }

        /// <summary>
        /// Sets a filter for the date before which the object should have been filter by date.
        /// </summary>
        /// <remarks>
        /// This is an exclusive limit. I.e. if the value is 2000-01-01, then only items created on or before 1999-12-31 will be included.
        /// </remarks>
        /// <param name="date">The date to use as the limit.</param>
        /// <param name="timezone">The timezone the filtering should use. Defaults to AET if null or not specified.</param>
        /// <returns>The current instance, to support fluent syntax.</returns>
        public EntityListFilters WithDateIsBeforeFilter(LocalDate date, DateTimeZone? timezone = null)
        {
            this.DateIsBeforeTicks = date.GetTicksAtStartOfDayInZone(timezone ?? Timezones.AET);
            return this;
        }
    }
}
