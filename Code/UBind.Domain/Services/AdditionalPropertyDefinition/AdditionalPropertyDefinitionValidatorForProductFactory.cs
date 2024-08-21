// <copyright file="AdditionalPropertyDefinitionValidatorForProductFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Factory validation for product.
    /// </summary>
    public class AdditionalPropertyDefinitionValidatorForProductFactory : IAdditionalPropertyDefinitionValidatorFactory
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;

        public AdditionalPropertyDefinitionValidatorForProductFactory(
            ICachingResolver cachingResolver,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
        {
            this.cachingResolver = cachingResolver;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionContextValidator BuildValidator()
        {
            return new AdditionalPropertyDefinitionForProductValidator(
                this.cachingResolver,
                this.additionalPropertyDefinitionRepository);
        }
    }
}
