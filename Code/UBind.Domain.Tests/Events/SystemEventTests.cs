// <copyright file="SystemEventTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Events
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class SystemEventTests
    {
        [Fact]
        public void AddRelationship_UpdatesRelationshipJson_WhenAddingFirstRelationship()
        {
            // Arrange
            var now = new TestClock().Now();
            var systemEvent = SystemEvent.CreateWithoutPayload(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                SystemEventType.PolicyIssued,
                now);

            // Act
            systemEvent.AddRelationship(new Relationship(
                TenantFactory.DefaultId,
                EntityType.Policy,
                Guid.NewGuid(),
                RelationshipType.PolicyEvent,
                EntityType.Event,
                systemEvent.Id,
                now));

            // Assert
            var result = JsonConvert.DeserializeObject<List<Relationship>>(systemEvent.RelationshipJson, CustomSerializerSetting.JsonSerializerSettings);
            result.Should().HaveCount(1);
        }

        [Fact]
        public void AddRelationship_UpdatesRelationshipJson_WhenAddingSecondRelationship()
        {
            // Arrange
            var now = new TestClock().Now();
            var systemEvent = SystemEvent.CreateWithoutPayload(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Staging,
                SystemEventType.PolicyIssued,
                now);

            // Act
            systemEvent.AddRelationship(new Relationship(
                TenantFactory.DefaultId,
                EntityType.Policy,
                Guid.NewGuid(),
                RelationshipType.PolicyEvent,
                EntityType.Event,
                systemEvent.Id,
                now));
            systemEvent.AddRelationship(new Relationship(
                TenantFactory.DefaultId,
                EntityType.PolicyTransaction,
                Guid.NewGuid(),
                RelationshipType.PolicyEvent,
                EntityType.Event,
                systemEvent.Id,
                now));

            // Assert
            var result = JsonConvert.DeserializeObject<List<Relationship>>(systemEvent.RelationshipJson, CustomSerializerSetting.JsonSerializerSettings);
            result.Should().HaveCount(2);
        }
    }
}
