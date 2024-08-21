// <copyright file="OrganisationReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;

    /// <summary>
    /// This class is needed because we need a data transfer object for organisation and its related entities.
    /// </summary>
    public class OrganisationReadModelWithRelatedEntities : IOrganisationReadModelWithRelatedEntities
    {
        /// <inheritdoc/>
        public OrganisationReadModel Organisation { get; set; }

        /// <inheritdoc/>
        public Tenant? Tenant { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TenantDetails> TenantDetails { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TextAdditionalPropertyValueReadModel> TextAdditionalPropertiesValues { get; set; }

        /// <inheritdoc/>
        public IEnumerable<StructuredDataAdditionalPropertyValueReadModel> StructuredDataAdditionalPropertyValues { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IAdditionalPropertyValueReadModel> AdditionalPropertyValues
        {
            get
            {
                return this.TextAdditionalPropertiesValues.Cast<IAdditionalPropertyValueReadModel>()
                    .Concat(this.StructuredDataAdditionalPropertyValues);
            }
        }
    }
}
