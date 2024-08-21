// <copyright file="IAdditionalPropertyDefinitionValidatorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    /// <summary>
    /// Contract of additional property validator factory.
    /// </summary>
    public interface IAdditionalPropertyDefinitionValidatorFactory
    {
        /// <summary>
        /// Creates an instance of objects that inherits AdditionalPropertyContextValidator.
        /// </summary>
        /// <returns>Concrete class of AdditionalPropertyContextValidator.</returns>
        AdditionalPropertyDefinitionContextValidator BuildValidator();
    }
}
