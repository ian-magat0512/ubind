// <copyright file="IsAdditionalPropertyValueUniqueQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyValue;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Services.AdditionalPropertyValue;

/// <summary>
/// Handler to check for duplicate additional properties.
/// </summary>
public class IsAdditionalPropertyValueUniqueQueryHandler
    : IQueryHandler<IsAdditionalPropertyValueUniqueQuery, bool>
{
    private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

    public IsAdditionalPropertyValueUniqueQueryHandler(PropertyTypeEvaluatorService propertyTypeEvaluatorService)
    {
        this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
    }

    public Task<bool> Handle(IsAdditionalPropertyValueUniqueQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var evaluator = this.propertyTypeEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
            request.PropertyType);
        return evaluator.IsAdditionalPropertyValueUnique(
            request.TenantId,
            request.AdditionalPropertyDefinitionId,
            request.Value,
            request.EntityId);
    }
}
