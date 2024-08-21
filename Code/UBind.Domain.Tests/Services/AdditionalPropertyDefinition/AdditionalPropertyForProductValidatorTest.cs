// <copyright file="AdditionalPropertyForProductValidatorTest.cs" company="uBind">
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
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdditionalPropertyForProductValidatorTest
    {
        private readonly Mock<IAdditionalPropertyDefinitionRepository> additionalPropertyDefinitionRepoMock;
        private readonly Mock<ICachingResolver> cachingResolverMock;
        private AdditionalPropertyDefinitionContextValidator additionalPropertyContextValidator;

        public AdditionalPropertyForProductValidatorTest()
        {
            this.additionalPropertyDefinitionRepoMock = new Mock<IAdditionalPropertyDefinitionRepository>(
                MockBehavior.Strict);
            this.cachingResolverMock = new Mock<ICachingResolver>(MockBehavior.Strict);

            this.additionalPropertyContextValidator = new AdditionalPropertyDefinitionForProductValidator(
                this.cachingResolverMock.Object, this.additionalPropertyDefinitionRepoMock.Object);
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldBeSuccessful_WhenPassesAllValidations()
        {
            // Assign
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var productId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            this.cachingResolverMock.Setup(
                m => m.GetProductOrNull(
                    It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(product);

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    tenantId,
                    tenantId,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnCreate(
                tenantId,
                modelOnCreate.Name,
                modelOnCreate.Alias,
                productId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.cachingResolverMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldRaiseException_WhenNameAlreadyExists()
        {
            // Assign
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            this.cachingResolverMock.Setup(
               m => m.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(product);

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = tenantId;
            }

            var sameNameModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            sameNameModel.Name = modelOnCreate.Name;
            sameNameModel.ParentContextId = tenantId;
            fakeList.Add(sameNameModel);

            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    tenantId,
                    tenantId,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnCreate(
                tenantId,
                modelOnCreate.Name,
                modelOnCreate.Alias,
                productId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenPropertyNameAlreadyExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.cachingResolverMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldRaiseException_WhenAliasAlreadyExists()
        {
            // Assign
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var readModelForAliasValidation = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            this.cachingResolverMock.Setup(m => m.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(product);

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = tenantId;
            }

            readModelForAliasValidation.Name = "XXXXX";
            readModelForAliasValidation.Alias = modelOnCreate.Alias;
            readModelForAliasValidation.ParentContextId = tenantId;
            fakeList.Add(readModelForAliasValidation);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    tenantId,
                    tenantId,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnCreate(
                tenantId,
                modelOnCreate.Name,
                modelOnCreate.Alias,
                productId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.cachingResolverMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldRaiseException_WhenProductNotExists()
        {
            // Assign
            var fakeId = Guid.Empty;
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var readModelByName = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            var readModelByAlias = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            var productId = Guid.NewGuid();
            Product product = null;

            this.cachingResolverMock.Setup(
               m => m.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(product);

            AdditionalPropertyDefinitionReadModel queriedModelById = AdditionalPropertyFakeData.
                CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            queriedModelById.ContextId = Guid.NewGuid();
            queriedModelById.ParentContextId = modelOnUpdate.ParentContextId;
            queriedModelById.ContextType = AdditionalPropertyDefinitionContextType.Product;
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                modelOnUpdate.Name,
                modelOnUpdate.Alias,
                productId,
                modelOnUpdate.EntityType,
                modelOnUpdate.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            this.AssertWhenProductIsNotFound(exception.Which.Error);
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldRaiseException_WhenNameAlreadyExists()
        {
            // Assign
            var fakeId = Guid.NewGuid();
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var queriedModelByName =
                AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            var queriedModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            queriedModelById.ParentContextId = modelOnUpdate.ParentContextId;
            queriedModelById.ContextType = AdditionalPropertyDefinitionContextType.Product;
            queriedModelById.ContextId = productId;
            queriedModelByName.Name = modelOnUpdate.Name;
            queriedModelByName.ParentContextId = modelOnUpdate.ParentContextId;

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = modelOnUpdate.ParentContextId;
            }

            fakeList.Add(queriedModelByName);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    It.IsAny<Guid>(),
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                modelOnUpdate.Name,
                modelOnUpdate.Alias,
                productId,
                modelOnUpdate.EntityType,
                modelOnUpdate.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenPropertyNameAlreadyExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldRaiseException_WhenAliasAlreadyExists()
        {
            // Assign
            var fakeId = Guid.NewGuid();
            var model = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            model.EntityType = AdditionalPropertyEntityType.Quote;
            model.ParentContextId = Guid.NewGuid();

            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var productId = Guid.NewGuid();
            var queriedModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            var queriedModelByAlias = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());

            queriedModelById.ParentContextId = model.ParentContextId.Value;
            queriedModelById.ContextId = productId;
            queriedModelByAlias.Name = "XXXXXX";
            queriedModelByAlias.Alias = model.Alias;
            queriedModelByAlias.ParentContextId = model.ParentContextId;

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = model.ParentContextId;
            }

            fakeList.Add(queriedModelByAlias);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    model.ParentContextId.Value,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                model.Name,
                model.Alias,
                productId,
                model.EntityType,
                model.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
        }

        [Fact]
        public void ThrowExceptionIfValidationFailsOnUpdate_ShouldBeSuccessful_WhenPassesAllValidations()
        {
            // Assign
            var fakeId = Guid.NewGuid();
            var model = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            model.EntityType = AdditionalPropertyEntityType.Quote;
            model.ParentContextId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var product = AdditionalPropertyFakeData.CreateFakeProduct();
            var readModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            readModelById.ParentContextId = model.ParentContextId;
            readModelById.ContextId = productId;

            this.cachingResolverMock.Setup(
               m => m.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(product);

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Product);
            this.additionalPropertyDefinitionRepoMock.Setup(
                 m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                     TenantFactory.DefaultId,
                     readModelById.ParentContextId.Value,
                     AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                model.Name,
                model.Alias,
                productId,
                model.EntityType,
                model.ParentContextId);

            // Assert
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.cachingResolverMock.VerifyAll();
        }

        private void AssertWhenProductIsNotFound(Error error)
        {
            error.Code.Should().Be("additionalproperty.product.not.found");
        }
    }
}
