// <copyright file="AdditionalPropertyDefinitionValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Client validator of validation factories.
    /// </summary>
    public class AdditionalPropertyDefinitionValidator : IAdditionalPropertyDefinitionValidator
    {
        private readonly IReadOnlyDictionary<AdditionalPropertyDefinitionContextType, IAdditionalPropertyDefinitionValidatorFactory>
            validatorFactoriesByContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionValidator"/> class.
        /// </summary>
        /// <param name="validatorFactoriesByContext">Consists of readonly mappings between the
        /// <see cref="AdditionalPropertyDefinitionContextType"/> and concrete validator factory.</param>
        public AdditionalPropertyDefinitionValidator(
            IReadOnlyDictionary<AdditionalPropertyDefinitionContextType, IAdditionalPropertyDefinitionValidatorFactory>
                validatorFactoriesByContext)
        {
            this.validatorFactoriesByContext = validatorFactoriesByContext;
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionContextValidator GetValidatorByContextType(
            AdditionalPropertyDefinitionContextType contextType)
        {
            if (this.validatorFactoriesByContext.TryGetValue(
                 contextType,
                 out IAdditionalPropertyDefinitionValidatorFactory additionalPropertyValidatorFactory))
            {
                return additionalPropertyValidatorFactory.BuildValidator();
            }

            throw new ErrorException(Errors.AdditionalProperties.InvalidContext(contextType));
        }
    }
}
