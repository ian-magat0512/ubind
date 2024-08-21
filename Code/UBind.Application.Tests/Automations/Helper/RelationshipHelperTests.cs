// <copyright file="RelationshipHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Helper;

using FluentAssertions;
using UBind.Application.Automation;
using UBind.Application.Automation.Email;
using UBind.Application.Automation.Helper;
using UBind.Application.Automation.Providers;
using UBind.Application.Tests.Automations.Fakes;
using UBind.Domain;
using UBind.Domain.Tests.Fakes;
using Xunit;
using UBind.Domain.SerialisedEntitySchemaObject;

public class RelationshipHelperTests
{
    private readonly Domain.Product.Product product;
    private Guid organisationId = Guid.NewGuid();
    private Guid tenantId = Guid.NewGuid();

    public RelationshipHelperTests()
    {
        this.product = ProductFactory.Create(this.tenantId, Guid.NewGuid());
    }

    [Fact]
    public async Task ResolveMessageRelationshipTest_ReturnsExpectedDefaultRelationshipFromContext()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            null, null, this.tenantId, this.organisationId, this.product.Id, Domain.DeploymentEnvironment.Development);
        IProviderContext providerContext = new ProviderContext(automationData);
        var expectedResult = new
        {
            Count = 2,
            RelationshipTypes = new List<RelationshipType>()
            {
                RelationshipType.OrganisationMessage,
                RelationshipType.ProductMessage
            },
        };

        // Act
        var createdRelationship = await RelationshipHelper.ResolveMessageRelationships(providerContext, null, this.tenantId);
        var result = new
        {
            Count = createdRelationship?.Count,
            RelationshipTypes = createdRelationship?.Select(r => r.RelationshipType).ToList()
        };

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ResolveMessageRelationshipTest_ReturnsDefinedRelationship()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            null, null, this.tenantId, this.organisationId, this.product.Id, Domain.DeploymentEnvironment.Development);
        IProviderContext providerContext = new ProviderContext(automationData);
        var policyEntity = new StaticProvider<Data<IEntity>>(new Policy(Guid.NewGuid()));
        var customerEntity = new StaticProvider<Data<IEntity>>(new Customer(Guid.NewGuid()));
        var organisationEntity = new StaticProvider<Data<IEntity>>(new Organisation(Guid.NewGuid()));
        var definedRelationships = new List<RelationshipConfiguration>
        {
          new RelationshipConfiguration(
                RelationshipType.PolicyMessage,
                policyEntity,
                null),
          new RelationshipConfiguration(
                RelationshipType.MessageRecipient,
                null,
                customerEntity),
          new RelationshipConfiguration(
                RelationshipType.MessageSender,
                null,
                organisationEntity),
        };
        var expectedResult = new
        {
            Count = 3,
            RelationshipTypes = new List<RelationshipType>()
            {
                RelationshipType.PolicyMessage,
                RelationshipType.MessageRecipient,
                RelationshipType.MessageSender,
            },
        };

        // Act
        var createdRelationship = await RelationshipHelper.ResolveMessageRelationships(providerContext, definedRelationships, this.tenantId);
        var result = new
        {
            Count = createdRelationship?.Count,
            RelationshipTypes = createdRelationship?.Select(r => r.RelationshipType).ToList()
        };

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task ResolveMessageRelationshipTest_ReturnsEmptyRelationship()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            null, null, this.tenantId, this.organisationId, this.product.Id, Domain.DeploymentEnvironment.Development);
        IProviderContext providerContext = new ProviderContext(automationData);
        var emptyRelationships = new List<RelationshipConfiguration>();
        var expectedResult = new
        {
            Count = 0,
            RelationshipTypes = new List<RelationshipType>()
        };

        // Act
        var createdRelationship = await RelationshipHelper.ResolveMessageRelationships(providerContext, emptyRelationships, this.tenantId);
        var result = new
        {
            Count = createdRelationship?.Count,
            RelationshipTypes = createdRelationship?.Select(r => r.RelationshipType).ToList()
        };

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
    }
}
