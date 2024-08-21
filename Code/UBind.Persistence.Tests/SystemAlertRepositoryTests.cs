// <copyright file="SystemAlertRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using FluentAssertions;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for System Alert Repository.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class SystemAlertRepositoryTests
    {
        [Fact]
        public void GetSystemAlertById_RecordsHasNewIds_WhenCreatingNewInstancesWithTenantParameterOnly()
        {
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var systemAlert = new SystemAlert(TenantFactory.DefaultId, null, SystemAlertType.ClaimNumbers, stack.Clock.Now());
                stack.SystemAlertRepository.AddAlert(systemAlert);
                stack.SystemAlertRepository.SaveChanges();

                // Act
                var retrievedSystemAlert = stack.SystemAlertRepository.GetSystemAlertById(systemAlert.TenantId, systemAlert.Id);

                // Assert
                retrievedSystemAlert.Should().NotBeNull();
                retrievedSystemAlert.ProductId.Should().BeNull();
                retrievedSystemAlert.TenantId.Should().Be(TenantFactory.DefaultId);
            }
        }

        [Fact]
        public void GetSystemAlertById_RecordsHasNewIds_WhenCreatingNewInstancesWithTenantAndProductParameter()
        {
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                // Arrange
                var systemAlert = new SystemAlert(TenantFactory.DefaultId, ProductFactory.DefaultId, SystemAlertType.ClaimNumbers, stack.Clock.Now());
                stack.SystemAlertRepository.AddAlert(systemAlert);
                stack.SystemAlertRepository.SaveChanges();

                // Act
                var retrievedSystemAlert = stack.SystemAlertRepository.GetSystemAlertById(systemAlert.TenantId, systemAlert.Id);

                // Assert
                retrievedSystemAlert.Should().NotBeNull();
                retrievedSystemAlert.ProductId.Should().Be(ProductFactory.DefaultId);
                retrievedSystemAlert.TenantId.Should().Be(TenantFactory.DefaultId);
            }
        }
    }
}
