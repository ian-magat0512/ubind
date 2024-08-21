// <copyright file="UserAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Entities.User
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Helpers;
    using Xunit;
    using static UBind.Domain.Aggregates.User.UserAggregate;

    public class UserAggregateTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Guid userOrganisationId = Guid.NewGuid();
        private readonly Guid tenantId = Guid.NewGuid();

        [Fact]
        public void EventArray_RoundtripsToJsonCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleAddedEvent = new UserAggregate.RoleAddedEvent(this.tenantId, userId, "foo", this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var userBlockedEvent = new UserAggregate.UserBlockedEvent(this.tenantId, userId, Guid.NewGuid(), default, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var events = new List<object> { roleAddedEvent, userBlockedEvent };
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
            };

            // Act
            var json = JsonConvert.SerializeObject(events, settings);
            var result = JsonConvert.DeserializeObject<List<object>>(json, settings);

            // Assert
            result.Count.Should().Be(2);
            result[0].Should().BeOfType<RoleAddedEvent>();
            result[1].Should().BeOfType<UserBlockedEvent>();
        }

        [Fact]
        public void AddingActivationInvitation_ShouldNot_Throw_WhenValidatingNewActivationId()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                 this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var invitationId = userAggregate.CreateActivationInvitation(this.performingUserId, this.clock.Now());

            // Act
            Action act = () => userAggregate.VerifyActivationInvitation(invitationId, this.clock.Now());

            // Assert
            act.Should().NotThrow<ErrorException>();
        }

        [Fact]
        public void AddingMultipleActivationInvitation_ShouldNot_Throw_WhenValidatingOldActivationId()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var oldInvitationId = userAggregate.CreateActivationInvitation(this.performingUserId, this.clock.Now());
            var newInvitationId = userAggregate.CreateActivationInvitation(this.performingUserId, this.clock.Now());

            // Act
            Action act = () => userAggregate.VerifyActivationInvitation(oldInvitationId, this.clock.Now());

            // Assert
            act.Should().NotThrow<ErrorException>();
        }

        [Fact]
        public void AddingMultipleActivationInvitation_Should_Throw_WhenValidatingNonExistsInvitationId()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var oldInvitationId = userAggregate.CreateActivationInvitation(this.performingUserId, this.clock.Now());
            var newInvitationId = userAggregate.CreateActivationInvitation(this.performingUserId, this.clock.Now());
            var nonExistingId = Guid.NewGuid();

            // Act
            Action act = () => userAggregate.VerifyActivationInvitation(nonExistingId, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("user.activation.token.not.found");
        }

        [Fact]
        public void AssignRole_Throws_WhenUserIsFromClientTenantAndRoleIsUBindRole()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var roleTenantId = Guid.NewGuid().ToString();
            var role = RoleHelper.CreateRole(
                Tenant.MasterTenantId, Guid.NewGuid(), "Test uBind role", "uBind Role for testing", this.clock.Now());

            // Act
            Action act = () => userAggregate.AssignRole(role, this.performingUserId, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("tenant.object.mismatch");
        }

        [Fact]
        public void AssignRole_Throws_WhenRoleIsFromWrongTenant()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                 this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var roleTenantId = Guid.NewGuid();
            var role = RoleHelper.CreateRole(
                roleTenantId, this.userOrganisationId, "Test role", "Role for testing", this.clock.Now());

            // Act
            Action act = () => userAggregate.AssignRole(role, this.performingUserId, this.clock.Now());

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("tenant.object.mismatch");
        }

        [Fact]
        public void AssignRole_Succeeds_WhenUserFromClientTenantIsAssignedClientRole()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                 this.tenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                this.tenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var role = RoleHelper.CreateRole(
                this.tenantId, this.userOrganisationId, "Test role", "Role for testing", this.clock.Now());

            // Act
            userAggregate.AssignRole(role, this.performingUserId, this.clock.Now());

            // Assert
            userAggregate.RoleIds.Should().Contain(role.Id);
        }

        [Fact]
        public void AssignRole_Succeeds_WhenUserFromMasterTenantIsAssignedUBindRole()
        {
            // Arrange
            var personAggregate = PersonAggregate.CreatePerson(
                  Tenant.MasterTenantId, this.userOrganisationId, this.performingUserId, this.clock.Now());
            var userAggregate = UserAggregate.CreateUser(
                personAggregate.TenantId, Guid.NewGuid(), UserType.Client, personAggregate, this.performingUserId, null, this.clock.Now());
            var role = RoleHelper.CreateRole(
                Tenant.MasterTenantId, this.userOrganisationId, "Test role", "Role for testing", this.clock.Now());

            // Act
            userAggregate.AssignRole(role, this.performingUserId, this.clock.Now());

            // Assert
            userAggregate.RoleIds.Should().Contain(role.Id);
        }
    }
}
