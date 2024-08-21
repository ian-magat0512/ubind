// <copyright file="AdditionalPropertyDefinitionValidatorForTenantFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Factory class in creating a concrete validator for tenant.
    /// </summary>
    public class AdditionalPropertyDefinitionValidatorForTenantFactory : IAdditionalPropertyDefinitionValidatorFactory
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionValidatorForTenantFactory"/> class.
        /// </summary>
        /// <param name="tenantRepository">Repository for tenant.</param>
        /// <param name="additionalPropertyDefinitionRepository">Additional property repository.</param>
        public AdditionalPropertyDefinitionValidatorForTenantFactory(
            ITenantRepository tenantRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
        {
            this.tenantRepository = tenantRepository;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionContextValidator BuildValidator()
        {
            return new AdditionalPropertyDefinitionForTenantValidator(
                this.tenantRepository,
                this.additionalPropertyDefinitionRepository);
        }
    }
}
