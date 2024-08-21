// <copyright file="RoleRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for Role Repository.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class RoleRepositoryTests
    {
        [Fact]
        public void GetRoleById_RecordsHasNewIds_WhenCreatingNewInstances()
        {
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var tenantId = Guid.NewGuid();
                var role = new Role(tenantId, Guid.NewGuid(), Domain.Permissions.DefaultRole.Broker, stack.Clock.Now());
                stack.RoleRepository.Insert(role);
                stack.RoleRepository.SaveChanges();

                // Act
                var retrievedRole = stack.RoleRepository.GetRoleById(tenantId, role.Id);

                // Assert
                retrievedRole.Should().NotBeNull();
                retrievedRole.TenantId.Should().Be(tenantId);
            }
        }
    }
}
