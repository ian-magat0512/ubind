// <copyright file="UserReadModelUpdateRepositoryIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.ReadModels
{
    using System;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using UBind.Persistence.ReadModels;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class UserReadModelUpdateRepositoryIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void Single_LazilyLoadsNavigationProperties_WhenReadModelSupportsLazyLoading()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clock = SystemClock.Instance;
            var tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var role1 = RoleHelper.CreateRole(
                    tenant.Id, tenant.Details.DefaultOrganisationId, "Test role 1", "Test role description 1", clock.Now());
                var role2 = RoleHelper.CreateRole(
                    tenant.Id, tenant.Details.DefaultOrganisationId, "Test role 1", "Test role description 1", clock.Now());
                dbContext.Roles.Add(role1);
                dbContext.Roles.Add(role2);
                var sut = new ReadModelUpdateRepository<UserReadModel>(dbContext);
                var personAggregate = PersonAggregate.CreatePerson(
                    tenant.Id, tenant.Details.DefaultOrganisationId, this.performingUserId, clock.Now());
                var personData = new PersonData(personAggregate);
                var userReadModel = new UserReadModel(
                    userId, personData, default, null, clock.Now(), Domain.UserType.Client);
                userReadModel.Roles.Add(role1);
                userReadModel.Roles.Add(role2);
                sut.Add(userReadModel);
                dbContext.SaveChanges();
            }

            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new ReadModelUpdateRepository<UserReadModel>(dbContext);

                // Act
                var model = sut.GetById(tenantId, userId);

                // Assert
                Assert.True(model.Roles.Any());
            }
        }
    }
}
