// <copyright file="CustomerReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// Read model for users.
    /// </summary>
    public class CustomerReadModel : EntityReadModel<Guid>
    {
        /// <summary>
        /// Initializes static properties.
        /// </summary>
        static CustomerReadModel()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerReadModel"/> class.
        /// </summary>
        /// <param name="id">The ID of the customer.</param>
        /// <param name="personReadModel">The primary person read model of the customer.</param>
        /// <param name="environment">The environment the customer belongs in.</param>
        /// <param name="createdTimestamp">The time the customer was created.</param>
        /// <param name="isTestData">A value indicating whether to return a test data.</param>
        public CustomerReadModel(
            Guid id,
            PersonReadModel personReadModel,
            DeploymentEnvironment environment,
            Guid? portalId,
            Instant createdTimestamp,
            bool isTestData)
            : base(personReadModel.TenantId, id, createdTimestamp)
        {
            this.PrimaryPersonId = personReadModel.Id;
            this.OrganisationId = personReadModel.OrganisationId;
            this.Environment = environment;
            this.PortalId = portalId;
            this.IsTestData = isTestData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerReadModel"/> class.
        /// </summary>
        /// <param name="id">The customer id.</param>
        protected CustomerReadModel(Guid id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected CustomerReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the person's organisation Id.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the collection of person.
        /// </summary>
        public virtual ICollection<PersonReadModel> People { get; set; } = new Collection<PersonReadModel>();

        /// <summary>
        /// Gets or sets the ID of the person the user refers to.
        /// </summary>
        public Guid PrimaryPersonId { get; set; }

        /// <summary>
        /// Gets the primary person.
        /// </summary>
        public PersonReadModel PrimaryPerson => this.People.SingleOrDefault(p => p.Id == this.PrimaryPersonId);

        /// <summary>
        /// Gets or sets the environment the customer sits in.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the Id of the user who owns this customer.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person who owns this customer.
        /// </summary>
        public Guid OwnerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who owns this customer.
        /// </summary>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the customer would log into by default,
        /// If there is no specific portal required for a given product.
        /// This would be null if the customer doesn't log into a portal, or the customer
        /// is expected to login to the default portal for the tenancy.
        /// This needed for the generation of links in emails, e.g. the user activation link.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the customer is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
