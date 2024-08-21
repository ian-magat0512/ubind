// <copyright file="TextAdditionalPropertyValueProcessor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Helpers;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Evaluator for default value in type text.
    /// </summary>
    public class TextAdditionalPropertyValueProcessor : IAdditionalPropertyValueProcessor
    {
        private readonly ITextAdditionalPropertyValueReadModelRepository readModelRepository;
        private readonly IClock clock;
        private readonly ITextAdditionalPropertyValueAggregateRepository aggregateRepository;
        private readonly IWritableReadModelRepository<TextAdditionalPropertyValueReadModel> writableReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAdditionalPropertyValueProcessor"/> class.
        /// </summary>
        /// <param name="readModelRepository">Text additional property value repository.</param>
        /// <param name="clock">Timestamp.</param>
        /// <param name="aggregateRepository">Text additional property aggregate repository.</param>
        public TextAdditionalPropertyValueProcessor(
            ITextAdditionalPropertyValueReadModelRepository readModelRepository,
            IClock clock,
            ITextAdditionalPropertyValueAggregateRepository aggregateRepository,
            IWritableReadModelRepository<TextAdditionalPropertyValueReadModel> writableReadModelRepository)
        {
            this.readModelRepository = readModelRepository;
            this.clock = clock;
            this.aggregateRepository = aggregateRepository;
            this.writableReadModelRepository = writableReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task SetNewValueOnEntityForAdditionalPropertyDefinitionForNonAggregateEntity(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value)
        {
            async Task<TextAdditionalPropertyValue> Save()
            {
                var additionalPropertyValue = new TextAdditionalPropertyValue(
                    tenantId,
                    additionalPropertyDefinitionId,
                    value,
                    entityId,
                    this.clock.GetCurrentInstant());

                await this.aggregateRepository.Save(additionalPropertyValue);
                return additionalPropertyValue;
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(Save);
        }

        /// <inheritdoc/>
        public void CreateNewAdditionalPropertyValueForAggregateEntity(
            Guid id,
            IAdditionalPropertyValue additionalPropertyValue)
        {
            this.writableReadModelRepository.Add(
                new TextAdditionalPropertyValueReadModel(
                    additionalPropertyValue.TenantId,
                    additionalPropertyValue.EntityId,
                    id,
                    additionalPropertyValue.AdditionalPropertyDefinitionId,
                    additionalPropertyValue.Value,
                    this.clock.GetCurrentInstant()));
        }

        public void DeleteAdditionalPropertyValueForAggregateEntity(Guid tenantId, Guid id)
        {
            this.writableReadModelRepository.DeleteById(tenantId, id);
        }

        /// <inheritdoc/>
        public async Task<Dto.AdditionalPropertyValueDto> GetAdditionalPropertyValue(
            Guid tenantId, Guid additionalPropertyDefinitionId, Guid entityId)
        {
            var readModel = await this.readModelRepository
                .GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                tenantId, entityId, additionalPropertyDefinitionId);
            return readModel;
        }

        /// <inheritdoc/>
        public async Task<List<Dto.AdditionalPropertyValueDto>> GetAdditionalPropertyValues(
            Guid tenantId,
            IAdditionalPropertyValueListFilter queryModel)
        {
            var readModels = await this.readModelRepository.GetAdditionalPropertyValuesBy(
                tenantId,
                queryModel);
            return readModels;
        }

        public async Task<bool> IsAdditionalPropertyValueUnique(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            string? value,
            Guid? entityId)
        {
            var readModels = await this.readModelRepository.IsAdditionalPropertyValueUnique(
                tenantId,
                additionalPropertyDefinitionId,
                value,
                entityId);
            return readModels;
        }

        /// <inheritdoc/>
        public async Task<bool> EntityHasAdditionalPropertyValue(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            Guid entityId)
        {
            var readModel = await this.readModelRepository
                .GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                tenantId, entityId, additionalPropertyDefinitionId);
            return readModel?.Id != null;
        }

        /// <inheritdoc/>
        public void UpdateAdditionalPropertyValueForAggregateEntity(Guid tenantId, Guid additionalPropertyValueId, string value)
        {
            var readModel = this.writableReadModelRepository.GetById(tenantId, additionalPropertyValueId);
            readModel.Value = value;
        }

        /// <inheritdoc/>
        public async Task UpdateValueForNonAggregateEntity(Guid tenantId, Guid id, string value)
        {
            var textAdditionalPropertyValueAggregate = this.aggregateRepository.GetById(tenantId, id);
            await this.UpdateAdditionalPropertyValue(tenantId, id, value, textAdditionalPropertyValueAggregate);
        }

        private async Task UpdateAdditionalPropertyValue(Guid tenantId, Guid id, string value, TextAdditionalPropertyValue textAdditionalPropertyValueAggregate)
        {
            async Task<TextAdditionalPropertyValue> UpdateValue()
            {
                textAdditionalPropertyValueAggregate.UpdateValue(
                    id,
                    value,
                    this.clock.GetCurrentInstant());
                await this.aggregateRepository.Save(textAdditionalPropertyValueAggregate);
                return textAdditionalPropertyValueAggregate;
            }

            await ConcurrencyPolicy.ExecuteWithRetriesAsync(
                UpdateValue,
                () => this.aggregateRepository.GetById(tenantId, id));
        }
    }
}
