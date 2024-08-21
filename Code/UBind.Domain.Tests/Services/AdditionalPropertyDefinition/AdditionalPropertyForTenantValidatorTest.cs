// <copyright file="AdditionalPropertyForTenantValidatorTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyDefinition
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdditionalPropertyForTenantValidatorTest
    {
        private readonly Mock<ITenantRepository> tenantRepositoryMock;
        private readonly Mock<IAdditionalPropertyDefinitionRepository> additionalPropertyDefinitionRepositoryMock;
        private readonly AdditionalPropertyDefinitionContextValidator additionalPropertyForTenantValidator;

        public AdditionalPropertyForTenantValidatorTest()
        {
            this.tenantRepositoryMock = new Mock<ITenantRepository>(MockBehavior.Strict);
            this.additionalPropertyDefinitionRepositoryMock = new Mock<IAdditionalPropertyDefinitionRepository>(
                MockBehavior.Strict);

            this.additionalPropertyForTenantValidator =
                new AdditionalPropertyDefinitionForTenantValidator(
                    this.tenantRepositoryMock.Object,
                    this.additionalPropertyDefinitionRepositoryMock.Object);
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldReturnException_WhenPropertyNameAlreadyExists()
        {
            // Assign
            var additionalPropertyReadModel = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            var tenant = AdditionalPropertyFakeData.CreateFakeTenant();
            var contextId = additionalPropertyReadModel.ContextId;
            var model = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyModelOnCreate();
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Tenant);

            this.tenantRepositoryMock.Setup(t => t.GetTenantById(It.IsAny<Guid>())).Returns(tenant);

            additionalPropertyReadModel.Name = model.Name;
            fakeList.Add(additionalPropertyReadModel);

            this.additionalPropertyDefinitionRepositoryMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnCreate(
                TenantFactory.DefaultId,
                model.Name,
                model.Alias,
                contextId,
                AdditionalPropertyEntityType.Quote,
                null);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenPropertyNameAlreadyExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldReturnException_WhenAliasAlreadyExists()
        {
            // Assign
            AdditionalPropertyDefinitionReadModel additionalPropertyReadModel = null;
            var tenant = AdditionalPropertyFakeData.CreateFakeTenant();
            var model = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyModelOnCreate();
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Tenant);

            this.tenantRepositoryMock.Setup(t => t.GetTenantById(It.IsAny<Guid>())).Returns(tenant);

            additionalPropertyReadModel = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            additionalPropertyReadModel.Name = "XXXXXXX";
            additionalPropertyReadModel.Alias = model.Alias;
            fakeList.Add(additionalPropertyReadModel);
            this.additionalPropertyDefinitionRepositoryMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnCreate(
                TenantFactory.DefaultId,
                model.Name,
                model.Alias,
                Guid.NewGuid(),
                AdditionalPropertyEntityType.Quote,
                null);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldReturnException_WhenTenantNotExists()
        {
            // Assign
            Tenant tenant = null;
            var model = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyModelOnCreate();

            this.tenantRepositoryMock.Setup(t => t.GetTenantById(It.IsAny<Guid>())).Returns(tenant);

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnCreate(
                TenantFactory.DefaultId,
                model.Name,
                model.Alias,
                Guid.NewGuid(),
                AdditionalPropertyEntityType.Quote,
                null);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            this.AssertWhenTenantIsNull(exception.Which.Error);
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldReturnException_WhenValidationFailsOnTenantMissing()
        {
            // Assign
            var fakeId = Guid.NewGuid();
            var model = AdditionalPropertyFakeData.
              CreateFakeAdditionalPropertyModelOnUpdate();
            model.EntityType = AdditionalPropertyEntityType.Quote;
            var fakeQueryable = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Tenant)
                .AsQueryable();
            AdditionalPropertyDefinitionReadModel additionalPropertyReadModel =
                AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);

            this.additionalPropertyDefinitionRepositoryMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeQueryable);

            Tenant tenant = null;
            this.tenantRepositoryMock.Setup(t => t.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                model.Name,
                model.Alias,
                Guid.NewGuid(),
                model.EntityType,
                model.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            this.AssertWhenTenantIsNull(exception.Which.Error);
            this.additionalPropertyDefinitionRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldReturnException_WhenNameAlreadyExists()
        {
            // Assign
            var model = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            model.EntityType = AdditionalPropertyEntityType.Quote;
            var fakeId = Guid.NewGuid();
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Tenant);

            var queriedReadModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            var queriedReadModelOnSequenceTwo = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());

            // Mocking the data by adding same
            var mockSameNameModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            mockSameNameModel.Name = queriedReadModel.Name;
            fakeList.Add(mockSameNameModel);

            this.additionalPropertyDefinitionRepositoryMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                model.Name,
                model.Alias,
                Guid.NewGuid(),
                model.EntityType,
                model.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenPropertyNameAlreadyExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldReturnException_WhenAliasAlreadyExists()
        {
            // Assign
            var model = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyModelOnUpdate();
            model.EntityType = AdditionalPropertyEntityType.Quote;
            var fakeId = Guid.NewGuid();
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Tenant);

            var queriedModelInSequenceOne = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                fakeId);

            var mockModelWithSameAlias = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());
            mockModelWithSameAlias.Name = "XXXXXXX";
            mockModelWithSameAlias.Alias = model.Alias;
            fakeList.Add(mockModelWithSameAlias);

            this.additionalPropertyDefinitionRepositoryMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyForTenantValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                model.Name,
                model.Alias,
                Guid.NewGuid(),
                model.EntityType,
                model.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepositoryMock.VerifyAll();
        }

        private void AssertWhenTenantIsNull(Domain.Error error)
        {
            Assert.Equal("additional.property.tenant.not.found", error.Code);
        }
    }
}
