// <copyright file="GetAdditionalPropertyValuesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyValue
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Handler for query <see cref="GetAdditionalPropertyValuesQuery"/>.
    /// </summary>
    public class GetAdditionalPropertyValuesQueryHandler
        : IQueryHandler<GetAdditionalPropertyValuesQuery, List<AdditionalPropertyValueDto>>
    {
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAdditionalPropertyValuesQueryHandler"/> class.
        /// </summary>
        /// <param name="propertyTypeEvaluatorService">Service that builds the specific type of evaluator.</param>
        public GetAdditionalPropertyValuesQueryHandler(PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
        }

        /// <inheritdoc/>
        public Task<List<AdditionalPropertyValueDto>> Handle(
            GetAdditionalPropertyValuesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var evaluator = this.propertyTypeEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
                request.PropertyType);
            return evaluator.GetAdditionalPropertyValues(request.TenantId, request);
        }
    }
}
