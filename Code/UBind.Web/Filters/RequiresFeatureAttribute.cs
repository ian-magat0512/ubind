// <copyright file="RequiresFeatureAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System;
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Domain;

    /// <summary>
    /// Attribute to check features on api endpoints.
    /// </summary>
    public class RequiresFeatureAttribute : Attribute, IFilterMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresFeatureAttribute"/> class.
        /// </summary>
        /// <param name="feature">The feature.</param>
        public RequiresFeatureAttribute(Feature feature)
        {
            this.Feature = feature;
        }

        /// <summary>
        /// Gets the feature name.
        /// </summary>
        public Feature Feature { get; }
    }
}
