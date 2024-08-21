// <copyright file="GetPrimaryPersonForCustomerQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Person
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    ///  A query to return the primary person read model summary of the person's customer relationship.
    /// </summary>
    public class GetPrimaryPersonForCustomerQuery : IQuery<IPersonReadModelSummary?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrimaryPersonForCustomerQuery"/> class.
        /// </summary>
        /// /// <param name="customerId">The ID of the customer.</param>
        public GetPrimaryPersonForCustomerQuery(Guid tenantId, Guid customerId)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
        }

        /// <summary>
        /// Gets the Id of the tenant.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the Id of the customer.
        /// </summary>
        public Guid CustomerId { get; private set; }
    }
}
