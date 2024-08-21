// <copyright file="GetContextEntitiesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ContextEntity
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <summary>
    /// This class is needed because we need a query to search Australian business register by ABN.
    /// </summary>
    public class GetContextEntitiesQuery : IQuery<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetContextEntitiesQuery"/> class.
        /// </summary>
        public GetContextEntitiesQuery(
            ReleaseContext releaseContext,
            Guid organisationId,
            Guid entityId,
            WebFormAppType webFormAppType,
            QuoteType? quoteType = null)
        {
            this.ReleaseContext = releaseContext;
            this.OrganisationId = organisationId;
            this.EntityId = entityId;
            this.WebFormAppType = webFormAppType;
            this.QuoteType = quoteType;
        }

        public ReleaseContext ReleaseContext { get; }

        public Guid OrganisationId { get; }

        public Guid EntityId { get; }

        public WebFormAppType WebFormAppType { get; }

        public QuoteType? QuoteType { get; }
    }
}
