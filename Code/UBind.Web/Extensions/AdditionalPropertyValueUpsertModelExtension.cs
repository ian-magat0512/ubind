// <copyright file="AdditionalPropertyValueUpsertModelExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Web.ResourceModels;

    public static class AdditionalPropertyValueUpsertModelExtension
    {
        /// <summary>
        /// Convert the <see cref="AdditionalPropertyValueUpsertModel"/> to the domain.
        /// <see cref="Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel"/> collection.
        /// </summary>
        /// <param name="resourceModelProperties">
        /// List of <see cref="AdditionalPropertyValueUpsertModel"/>.
        /// </param>
        /// <returns>List of <see cref="Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel"/>.</returns>
        public static List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel> ToDomainAdditionalProperties(
            this List<ResourceModels.AdditionalPropertyValueUpsertModel> resourceModelProperties)
        {
            List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel> retValue =
                new List<Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel>();
            if (resourceModelProperties != null && resourceModelProperties.Any())
            {
                retValue = resourceModelProperties.Select(rmp => new Domain.Aggregates.AdditionalPropertyValue.AdditionalPropertyValueUpsertModel
                {
                    Value = rmp.Value,
                    DefinitionId = rmp.DefinitionId,
                    Type = rmp.PropertyType,
                }).ToList();
            }

            return retValue;
        }
    }
}
