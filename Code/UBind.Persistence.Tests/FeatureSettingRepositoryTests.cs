// <copyright file="FeatureSettingRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Integration tests for Claims Number Repository.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class FeatureSettingRepositoryTests
    {
        public FeatureSettingRepositoryTests()
        {
            var setting1 = "test";
            var setting2 = "test2";
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            if (!stack.DbContext.Settings.Any(x => x.Id == setting1))
            {
                stack.FeatureSettingRepository.Insert(new Setting(setting1, "test", "test", 0, stack.Clock.Now(), IconLibrary.IonicV4));
            }

            if (!stack.DbContext.Settings.Any(x => x.Id == setting2))
            {
                stack.FeatureSettingRepository.Insert(new Setting(setting2, "test2", "test2", 0, stack.Clock.Now(), IconLibrary.IonicV4));
            }

            stack.FeatureSettingRepository.SaveChanges();
        }

        [Fact]
        public void SetInitialSettingsByTenantIdOrOrganisationId_Successful_WhenTriggered()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var tenant = TenantFactory.Create(tenantId);
            stack.CreateTenant(tenant);

            // Act
            stack.FeatureSettingRepository.SetInitialSettings(tenantId);

            // Assert
            var settings = stack.FeatureSettingRepository.GetSettings(tenantId);
            settings.Where(x => x.Details.Tenant.Id == tenantId).Should().HaveCount(2);
        }

        [Fact]
        public void GetSettingsByTenantIdAndOrOrganisationId_NothingSet_IfNotSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            using (ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var tenant = TenantFactory.Create(tenantId);
                stack.CreateTenant(tenant);

                // Act
                var settings = stack.FeatureSettingRepository.GetSettings(tenantId);

                // Assert
                settings.Where(x => x?.Details?.Tenant.Id == tenantId).Should().HaveCount(0);
            }
        }
    }
}
