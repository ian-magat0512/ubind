// <copyright file="AdditionalPropertyForOrganisationValidatorTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services.AdditionalPropertyDefinition
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyDefinition;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AdditionalPropertyForOrganisationValidatorTest
    {
        private readonly Mock<IOrganisationReadModelRepository> organisationRepositoryMock;
        private readonly Mock<IAdditionalPropertyDefinitionRepository> additionalPropertyDefinitionRepoMock;
        private readonly Mock<ICachingResolver> cachingResolver;
        private readonly AdditionalPropertyDefinitionForOrganisationValidator additionalPropertyContextValidator;

        public AdditionalPropertyForOrganisationValidatorTest()
        {
            this.organisationRepositoryMock = new Mock<IOrganisationReadModelRepository>(MockBehavior.Strict);
            this.additionalPropertyDefinitionRepoMock = new Mock<IAdditionalPropertyDefinitionRepository>(
                MockBehavior.Strict);
            this.cachingResolver = new Mock<ICachingResolver>();

            this.additionalPropertyContextValidator = new AdditionalPropertyDefinitionForOrganisationValidator(
                this.organisationRepositoryMock.Object,
                this.additionalPropertyDefinitionRepoMock.Object);
        }

        [Fact]
        public void ThrowExceptionIfValidationFailsOnCreate_ShouldBeSuccessful_WhenPassesAllValidations()
        {
            // Assign
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var fakeOrgId = Guid.NewGuid();
            modelOnCreate.ParentContextId = fakeOrgId;
            var tenantId = Guid.NewGuid();

            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);

            this.organisationRepositoryMock.Setup(
                prm => prm.Get(
                    It.IsAny<Guid>(),
                    fakeOrgId)).
                Returns(organisation);
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    tenantId,
                    tenantId,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());

            // Act
            this.additionalPropertyContextValidator.ThrowIfValidationFailsOnCreate(
                tenantId,
                modelOnCreate.Name,
                modelOnCreate.Alias,
                fakeOrgId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.organisationRepositoryMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldRaiseException_WhenOrganisationNotExists()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();

            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            OrganisationReadModel organisation = null;

            this.organisationRepositoryMock.Setup(org => org.Get(It.IsAny<Guid>(), fakeOrgId)).
                Returns(organisation);
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnCreate(
                TenantFactory.DefaultId,
                modelOnCreate.Name,
                modelOnCreate.Alias,
                fakeOrgId,
                AdditionalPropertyEntityType.Quote,
                Guid.NewGuid());

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            this.AssertWhenOrganisationIsNotFound(exception.Which.Error);
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldRaiseException_WhenNameAlreadyExists()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);
            var readModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());

            this.organisationRepositoryMock.Setup(
                prm => prm.Get(It.IsAny<Guid>(), fakeOrgId)).Returns(organisation);
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            readModel.Name = modelOnCreate.Name;
            readModel.ParentContextId = tenantId;
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = tenantId;
            }

            fakeList.Add(readModel);
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
                fakeOrgId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenPropertyNameAlreadyExists(exception.Which.Error);
            this.organisationRepositoryMock.VerifyAll();
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnCreate_ShouldRaiseException_WhenAliasAlreadyExists()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var modelOnCreate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnCreate();
            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);
            var readModelForAliasValidation = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());

            this.organisationRepositoryMock.Setup(
                prm => prm.Get(It.IsAny<Guid>(), fakeOrgId)).Returns(organisation);
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            readModelForAliasValidation.Name = "XXXXXX";
            readModelForAliasValidation.Alias = modelOnCreate.Alias;
            readModelForAliasValidation.ParentContextId = tenantId;
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = tenantId;
            }

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
                fakeOrgId,
                AdditionalPropertyEntityType.Quote,
                tenantId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
            this.organisationRepositoryMock.VerifyAll();
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldRaiseException_WhenOrganisationNotExists()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();
            var fakeId = Guid.NewGuid();
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var readModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            readModelById.ContextId = fakeOrgId;
            readModelById.ContextType = AdditionalPropertyDefinitionContextType.Organisation;
            readModelById.ParentContextId = Guid.NewGuid();
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    modelOnUpdate.ParentContextId.Value,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());
            OrganisationReadModel organisation = null;

            this.organisationRepositoryMock.Setup(
                prm => prm.Get(It.IsAny<Guid>(), fakeOrgId)).Returns(organisation);
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            // Act
            var act = async () => await this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                modelOnUpdate.Name,
                modelOnUpdate.Alias,
                fakeOrgId,
                modelOnUpdate.EntityType,
                modelOnUpdate.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            this.AssertWhenOrganisationIsNotFound(exception.Which.Error);
        }

        [Fact]
        public async Task ThrowExceptionIfValidationFailsOnUpdate_ShouldRaiseException_WhenNameAlreadyExists()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();
            var fakeId = Guid.NewGuid();
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);
            var queriedModelByName = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(Guid.NewGuid());
            var queriedModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            queriedModelById.ContextId = fakeOrgId;
            queriedModelById.ParentContextId = Guid.NewGuid();
            queriedModelById.ContextType = AdditionalPropertyDefinitionContextType.Organisation;
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            queriedModelByName.Name = modelOnUpdate.Name;
            queriedModelByName.ParentContextId = modelOnUpdate.ParentContextId;
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
                fakeOrgId,
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
            var fakeOrgId = Guid.NewGuid();
            var fakeId = Guid.NewGuid();
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);
            var queriedModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            queriedModelById.ContextType = AdditionalPropertyDefinitionContextType.Organisation;
            queriedModelById.ParentContextId = modelOnUpdate.ParentContextId;
            queriedModelById.ContextId = fakeOrgId;

            var queriedModelByAlias = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());
            queriedModelByAlias.Name = "XXXXXX";
            queriedModelByAlias.Alias = modelOnUpdate.Alias;
            queriedModelByAlias.ParentContextId = modelOnUpdate.ParentContextId;
            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(
                AdditionalPropertyDefinitionContextType.Organisation);
            for (var i = 0; i < fakeList.Count; i++)
            {
                fakeList[i].ParentContextId = modelOnUpdate.ParentContextId;
            }

            fakeList.Add(queriedModelByAlias);
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
                fakeOrgId,
                modelOnUpdate.EntityType,
                modelOnUpdate.ParentContextId);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            AdditionalPropertyAssert.AssertWhenAliasExists(exception.Which.Error);
        }

        [Fact]
        public void ThrowExceptionIfValidationFailsOnUpdate_ShouldBeSuccessful_WhenPassesAllValidations()
        {
            // Assign
            var fakeOrgId = Guid.NewGuid();
            var fakeId = Guid.NewGuid();
            var modelOnUpdate = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyModelOnUpdate();
            modelOnUpdate.EntityType = AdditionalPropertyEntityType.Quote;
            modelOnUpdate.ParentContextId = Guid.NewGuid();
            var organisation = AdditionalPropertyFakeData.CreateFakeOrganisation(fakeOrgId);
            var readModelById = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            readModelById.ContextId = fakeOrgId;
            readModelById.ContextType = AdditionalPropertyDefinitionContextType.Organisation;
            readModelById.ParentContextId = modelOnUpdate.ParentContextId;

            this.organisationRepositoryMock.Setup(
                prm => prm.Get(It.IsAny<Guid>(), fakeOrgId)).Returns(organisation);

            var fakeList = AdditionalPropertyFakeData.GenerateFakeList(AdditionalPropertyDefinitionContextType.Organisation);
            this.additionalPropertyDefinitionRepoMock.Setup(
                m => m.GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                    TenantFactory.DefaultId,
                    readModelById.ParentContextId.Value,
                    AdditionalPropertyEntityType.Quote)).Returns(fakeList.AsQueryable());
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrThrow(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));
            this.cachingResolver.Setup(m => m.GetTenantByAliasOrNull(It.IsAny<string>()))
                .Returns(Task.FromResult(AdditionalPropertyFakeData.CreateFakeTenant()));

            // Act
            this.additionalPropertyContextValidator.ThrowIfValidationFailsOnUpdate(
                TenantFactory.DefaultId,
                fakeId,
                modelOnUpdate.Name,
                modelOnUpdate.Alias,
                fakeOrgId,
                modelOnUpdate.EntityType,
                modelOnUpdate.ParentContextId);

            // Assert
            this.additionalPropertyDefinitionRepoMock.VerifyAll();
            this.organisationRepositoryMock.VerifyAll();
        }

        private void AssertWhenOrganisationIsNotFound(Error error)
        {
            error.Code.Should().Be("additional.property.organisation.not.found");
        }
    }
}
