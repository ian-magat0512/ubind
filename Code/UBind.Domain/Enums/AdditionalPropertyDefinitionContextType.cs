// <copyright file="AdditionalPropertyDefinitionContextType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// This custom type is used in identifying which context (please see the items below) an additional property
    /// definition record will be mapped to. This type is important to further classify the additional properties in
    /// the database.
    /// </summary>
    public enum AdditionalPropertyDefinitionContextType
    {
        /// <summary>
        /// Tenant.
        /// </summary>
        [Description("Tenant")]
        Tenant,

        /// <summary>
        /// Organisation.
        /// </summary>
        [Description("Organisation")]
        Organisation,

        /// <summary>
        /// Product.
        /// </summary>
        [Description("Product")]
        Product,
    }
}
