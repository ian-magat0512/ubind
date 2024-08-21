// <copyright file="AdditionalPropertyDefinitionValidatorForOrganisationFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Validator for ogranisation.
    /// </summary>
    public class AdditionalPropertyDefinitionValidatorForOrganisationFactory : IAdditionalPropertyDefinitionValidatorFactory
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionValidatorForOrganisationFactory"/> class.
        /// </summary>
        /// <param name="organisationReadModelRepository">Organisation read model repository.</param>
        /// <param name="additionalPropertyDefinitionRepository">Additional property repository.</param>
        /// <param name="cachingResolver">Caching resolver.</param>
        public AdditionalPropertyDefinitionValidatorForOrganisationFactory(
            IOrganisationReadModelRepository organisationReadModelRepository,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            ICachingResolver cachingResolver)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionContextValidator BuildValidator()
        {
            return new AdditionalPropertyDefinitionForOrganisationValidator(
                this.organisationReadModelRepository,
                this.additionalPropertyDefinitionRepository);
        }
    }
}
