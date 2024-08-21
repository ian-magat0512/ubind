// <copyright file="AdditionalPropertyEntityTypeCategoryAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;
    using UBind.Domain.Enums;

    /// <summary>
    /// Attribute to specifiy the category of additional property entity type.
    /// </summary>
    public class AdditionalPropertyEntityTypeCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyEntityTypeCategoryAttribute"/> class.
        /// </summary>
        /// <param name="category">The category enum.</param>
        public AdditionalPropertyEntityTypeCategoryAttribute(AdditionalPropertyEntityTypeCategory category)
        {
            this.Category = category;
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public AdditionalPropertyEntityTypeCategory Category { get; set; }
    }
}
