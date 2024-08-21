// <copyright file="UpsertValuesToEntityCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.AdditionalPropertyValue
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Notification handler for <see cref="UpsertValuesToEntityCommand"/>.
    /// </summary>
    public class UpsertValuesToEntityCommandHandler : ICommandHandler<UpsertValuesToEntityCommand>
    {
        private readonly PropertyTypeEvaluatorService propertyEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsertValuesToEntityCommandHandler"/> class.
        /// </summary>
        /// <param name="evaluatorService"><see cref="PropertyTypeEvaluatorService"/>.</param>
        public UpsertValuesToEntityCommandHandler(
            PropertyTypeEvaluatorService evaluatorService)
        {
            this.propertyEvaluatorService = evaluatorService;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(UpsertValuesToEntityCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var valueTypeIdModel in request.ValueIdTypeModels)
            {
                var evaluator = this.propertyEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
                    valueTypeIdModel.Type);
                var additionalPropertyValueReadModel = await evaluator.GetAdditionalPropertyValue(
                    request.TenantId, valueTypeIdModel.DefinitionId, request.EntityId);
                if (additionalPropertyValueReadModel != null && additionalPropertyValueReadModel.Id.HasValue)
                {
                    await evaluator.UpdateValueForNonAggregateEntity(
                        request.TenantId,
                        additionalPropertyValueReadModel.Id.Value,
                        valueTypeIdModel.Value);
                }
                else
                {
                    await evaluator.SetNewValueOnEntityForAdditionalPropertyDefinitionForNonAggregateEntity(
                        request.TenantId, request.EntityId, valueTypeIdModel.DefinitionId, valueTypeIdModel.Value);
                }
            }

            return Unit.Value;
        }
    }
}
