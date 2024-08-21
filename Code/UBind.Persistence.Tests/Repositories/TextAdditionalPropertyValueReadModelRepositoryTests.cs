// <copyright file="TextAdditionalPropertyValueReadModelRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Repositories
{
    using System;
    using System.Data.Entity.Infrastructure;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Enums;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="DeploymentRepositoryTests"/>.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class TextAdditionalPropertyValueReadModelRepositoryTests
    {
        private readonly ApplicationStack stack;
        private readonly Guid? performingUserId = Guid.NewGuid();

        public TextAdditionalPropertyValueReadModelRepositoryTests()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
        }

        [Fact]
        public async Task GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity_ShouldSucceed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(tenantId, productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.TenantRepository.SaveChanges();
            var organisationAggregate = Organisation.CreateNewOrganisation(
               tenant.Id,
               tenant.Details.Alias,
               tenant.Details.Name,
               null,
               this.performingUserId,
               this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisationAggregate);

            var additionalPropertyDefinition = AdditionalPropertyDefinition.CreateForText(
                        tenantId,
                        "tenant-product-prop-one",
                        "Tenant Product Prop One",
                        AdditionalPropertyEntityType.Product,
                        AdditionalPropertyDefinitionContextType.Tenant,
                        false,
                        false,
                        product.Id,
                        tenant.Id,
                        string.Empty,
                        this.performingUserId,
                        this.stack.Clock.GetCurrentInstant());
            await this.stack.AdditionalPropertyDefinitionAggregateRepository.Save(additionalPropertyDefinition);
            var expectedValue = "Property Value";
            var additionalPropertyValue = new TextAdditionalPropertyValue(
                    tenantId,
                    additionalPropertyDefinition.Id,
                    expectedValue,
                    product.Id,
                    this.stack.Clock.GetCurrentInstant());

            await this.stack.TextAdditionalPropertyValueAggregateRepository.Save(additionalPropertyValue);

            // Act
            var result = await this.stack.TextAdditionalPropertyValueReadModelRepository
                .GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
                    tenantId, productId, additionalPropertyDefinition.Id);

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async Task GetAdditionalPropertyValuesByEntityId_ShouldSucceed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var product = ProductFactory.Create(tenantId, productId);
            this.stack.TenantRepository.Insert(tenant);
            this.stack.TenantRepository.SaveChanges();
            var organisationAggregate = Organisation.CreateNewOrganisation(
               tenant.Id,
               tenant.Details.Alias,
               tenant.Details.Name,
               null,
               this.performingUserId,
               this.stack.Clock.GetCurrentInstant());
            await this.stack.OrganisationAggregateRepository.Save(organisationAggregate);

            var tenantProductDefinition = AdditionalPropertyDefinition.CreateForText(
                        tenantId,
                        "tenant-product-prop-two",
                        "Tenant Product Prop Two",
                        AdditionalPropertyEntityType.Product,
                        AdditionalPropertyDefinitionContextType.Tenant,
                        false,
                        false,
                        product.Id,
                        tenant.Id,
                        string.Empty,
                        this.performingUserId,
                        this.stack.Clock.GetCurrentInstant());
            await this.stack.AdditionalPropertyDefinitionAggregateRepository.Save(tenantProductDefinition);
            var tenantOrganisationDefinition = AdditionalPropertyDefinition.CreateForText(
                        tenantId,
                        "tenant-organisation-prop-one",
                        "Tenant Organisation Prop One",
                        AdditionalPropertyEntityType.Organisation,
                        AdditionalPropertyDefinitionContextType.Tenant,
                        false,
                        false,
                        organisationAggregate.Id,
                        tenant.Id,
                        string.Empty,
                        this.performingUserId,
                        this.stack.Clock.GetCurrentInstant());
            await this.stack.AdditionalPropertyDefinitionAggregateRepository.Save(tenantOrganisationDefinition);

            var expectedValue = "Tenant Product Property Value";
            var tenantProductPropertyValue = new TextAdditionalPropertyValue(
                    tenantId,
                    tenantProductDefinition.Id,
                    expectedValue,
                    product.Id,
                    this.stack.Clock.GetCurrentInstant());
            await this.stack.TextAdditionalPropertyValueAggregateRepository.Save(tenantProductPropertyValue);
            var tenantOrganisationPropertyValue = new TextAdditionalPropertyValue(
                   tenantId,
                   tenantOrganisationDefinition.Id,
                   "Tenant Organsation Property Value",
                   organisationAggregate.Id,
                   this.stack.Clock.GetCurrentInstant());
            await this.stack.TextAdditionalPropertyValueAggregateRepository.Save(tenantOrganisationPropertyValue);

            // Act
            var result = await this.stack.TextAdditionalPropertyValueReadModelRepository
                .GetAdditionalPropertyValuesByEntityId(tenantId, productId);

            // Assert
            result.Should().NotBeNullOrEmpty().And.Contain(ap => ap.Value == expectedValue);
        }

        [Fact]
        public async Task Save_ThrowsDbUpdateException_ForDuplicateAdditionalPropertyValues()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            var additionalPropertyDefinitionId = Guid.NewGuid();
            var expectedValue = "Leon";

            // Create an additional property definition with the unique setting
            var additionalPropertyDefinition = AdditionalPropertyDefinition.CreateForText(
                tenantId,
                "tenant-product-prop-one",
                "Tenant Product Prop One",
                AdditionalPropertyEntityType.Product,
                AdditionalPropertyDefinitionContextType.Tenant,
                true, // Set the IsUnique property to true
                false,
                entityId,
                tenantId,
                string.Empty,
                this.performingUserId,
                this.stack.Clock.GetCurrentInstant());
            await this.stack.AdditionalPropertyDefinitionAggregateRepository.Save(additionalPropertyDefinition);

            // Create two additional property values with the same value
            var additionalPropertyValue1 = new TextAdditionalPropertyValue(
                tenantId,
                additionalPropertyDefinitionId,
                expectedValue,
                entityId,
                this.stack.Clock.GetCurrentInstant());

            var additionalPropertyValue2 = new TextAdditionalPropertyValue(
                tenantId,
                additionalPropertyDefinitionId,
                expectedValue,
                entityId,
                this.stack.Clock.GetCurrentInstant());

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await this.stack.TextAdditionalPropertyValueAggregateRepository.Save(additionalPropertyValue1);
                await this.stack.TextAdditionalPropertyValueAggregateRepository.Save(additionalPropertyValue2);
            });
        }
    }
}
