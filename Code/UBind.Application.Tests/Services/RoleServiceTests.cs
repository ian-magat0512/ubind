// <copyright file="RoleServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using Xunit;

    public class RoleServiceTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly IClock clock = SystemClock.Instance;
        private Mock<ICachingResolver> mockCachingResolver = new Mock<ICachingResolver>();
        private Mock<IUBindDbContext> mockDbContext = new Mock<IUBindDbContext>();
        private Mock<IUserSessionDeletionService> mockUserSessionDeletionService = new Mock<IUserSessionDeletionService>();

        public RoleServiceTests()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public void UpdateRole_Throws_WhenUserAttemptsToUpdateRoleInOtherTenant()
        {
            // Arrange
            var fooTenant = TenantFactory.Create();
            var role = RoleHelper.CreateRole(
                fooTenant.Id, fooTenant.Details.DefaultOrganisationId, "X", "Y", this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(fooTenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(fooTenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(fooTenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(fooTenant));

            var barTenant = TenantFactory.Create(Guid.NewGuid());

            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.UpdateRole(barTenant.Id, role.Id, "X'", "Y'");

            // Assert
            act.Should().Throw<ErrorException>();
        }

        [Fact]
        public void DeleteRole_Throws_WhenRoleIsAssignedToUsers()
        {
            // Arrange
            var tenant = TenantFactory.Create(Tenant.MasterTenantId);
            var role = RoleHelper.CreateRole(tenant.Id, tenant.Details.DefaultOrganisationId, "X", "Y", this.clock.Now());
            var assignedUser = this.CreateUserReadModel(tenant);
            role.Users.Add(assignedUser);
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.delete.role.in.use");
        }

        [Theory]
        [ClassData(typeof(NonPermanentClientRoleTestData))]
        public void DeleteRole_Successful_WhenUserAttemptsToDelete_NonPermanent_Client_Roles(Role role)
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            roleRepository.Setup(rr => rr.Delete(It.IsAny<Role>())).Returns(true);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            var result = sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            result.Should().Be(true);
        }

        [Theory]
        [ClassData(typeof(NonPermanentMasterRoleTestData))]
        public void DeleteRole_Successful_WhenUserAttemptsToDelete_NonPermanent_Master_Roles(Role role)
        {
            // Arrange
            var tenant = TenantFactory.Create(role.TenantId);
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            roleRepository.Setup(rr => rr.Delete(It.IsAny<Role>())).Returns(true);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            var userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Master,
                Guid.NewGuid(),
                default);
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            var result = sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            result.Should().Be(true);
        }

        [Fact]
        public void DeleteRole_Throws_WhenUserAttemptsToDelete_uBindAdminRole()
        {
            // Arrange
            var tenant = TenantFactory.Create(Tenant.MasterTenantId);
            var role = RoleHelper.CreateMasterAdminRole(this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));

            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.delete.permanent.role");
        }

        [Fact]
        public void DeleteRole_Throws_WhenUserAttemptsToDelete_ClientAdminRole()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.delete.permanent.role");
        }

        [Fact]
        public void DeleteRole_Throws_WhenUserAttemptsToDelete_CustomerRole()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateCustomerRole(tenant.Id, tenant.Details.DefaultOrganisationId, this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.DeleteRole(tenant.Id, role.Id);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("cannot.delete.permanent.role");
        }

        [Fact(Skip = "Failing test and needs to be fixed ASAP")]
        public void GetRoleByIdForUser_Throws_WhenUserAttemptsToAccessRoleInOtherTenant()
        {
            // Arrange
            var fooTenant = TenantFactory.Create();
            var role = RoleHelper.CreateRole(
                fooTenant.Id, fooTenant.Details.DefaultOrganisationId, "X", "Y", this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);

            var barTenant = TenantFactory.Create(Guid.NewGuid());
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(barTenant));
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(barTenant));

            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.GetRole(barTenant.Id, role.Id);

            // Assert
            act.Should().Throw<UnauthorizedException>();
        }

        [Fact]
        public void GetRoleByIdForUser_Throws_ForUnknownRole()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.GetRole(tenant.Id, Guid.NewGuid());

            // Assert
            act.Should().Throw<NotFoundException>();
        }

        [Fact]
        public void GetRoleByIdForUser_Succeeds_WhenUserAttemptsToAccessRoleInOwnTenant()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var role = RoleHelper.CreateRole(tenant.Id, tenant.Details.DefaultOrganisationId, "X", "Y", this.clock.Now());
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(rr => rr.GetRoleById(It.IsAny<Guid>(), role.Id)).Returns(role);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            var retrievedRole = sut.GetRole(tenant.Id, role.Id);

            // Assert
            Assert.Equal(role.Id, retrievedRole.Id);
        }

        [Fact]
        public async Task CreateRole_Throws_When_RoleName_Is_EmptyAsync()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            var userAuthData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Master,
                Guid.NewGuid(),
                default);
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Func<Task> act = async () => await sut.CreateRole(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                string.Empty,
                RoleType.Master,
                "Y");

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("role.name.cannot.be.blank");
        }

        [Fact]
        public async void CreateRole_CreatesRoleInCorrectTenant()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            var role = await sut.CreateRole(tenant.Id, tenant.Details.DefaultOrganisationId, "X", RoleType.Master, "Y");

            // Assert
            Assert.Equal(tenant.Id, role.TenantId);
            Assert.Equal(tenant.Details.DefaultOrganisationId, role.OrganisationId);
        }

        [Fact]
        public void CreateDefaultRolesForTenant_Throw_When_Tenant_Is_Null()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(c => c.Insert(It.IsAny<Role>()));
            roleRepository.Setup(c => c.SaveChanges());
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            Action act = () => sut.CreateDefaultRolesForTenant(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(act);
            exception.ParamName.Should().Be("tenant");
            exception.Message.Should().Contain("A null parameter was passed in CreateDefaultRolesForTenant.");
        }

        [Fact]
        public void CreateDefaultRolesForTenant_Succeed_And_Create_Default_Roles()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();
            roleRepository.Setup(c => c.IsNameInUse(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns(false);
            roleRepository.Setup(c => c.Insert(It.IsAny<Role>()));
            roleRepository.Setup(c => c.SaveChanges());
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            sut.CreateDefaultRolesForTenant(tenant);

            // Assert
            // verify all default client roles are added to repository and called once
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.TenantAdmin.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.Broker.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.UnderWriter.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.ClaimsAgent.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.Customer.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);

            var clientProductDeveloperRole = DefaultRole.ClientProductDeveloper.GetAttributeOfType<RoleInformationAttribute>();
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == clientProductDeveloperRole.Name && r.Type == clientProductDeveloperRole.RoleType)), Times.Once);

            // verify all default MASTER roles are NOT added.
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.MasterAdmin.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Never);

            var masterProductDeveloperRole = DefaultRole.MasterProductDeveloper.GetAttributeOfType<RoleInformationAttribute>();
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == masterProductDeveloperRole.Name && r.Type == masterProductDeveloperRole.RoleType)), Times.Never);
        }

        [Fact]
        public void CreateDefaultRolesForTenant_Succeed_When_ClientAdmin_And_Customer_Already_Exists()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var roleRepository = new Mock<IRoleRepository>();

            var clientAdmin = DefaultRole.TenantAdmin.GetAttributeOfType<RoleInformationAttribute>().Name;
            var broker = DefaultRole.Broker.GetAttributeOfType<RoleInformationAttribute>().Name;
            var underWriter = DefaultRole.UnderWriter.GetAttributeOfType<RoleInformationAttribute>().Name;
            var claimsAgent = DefaultRole.ClaimsAgent.GetAttributeOfType<RoleInformationAttribute>().Name;
            var productDeveloper = DefaultRole.ClientProductDeveloper.GetAttributeOfType<RoleInformationAttribute>().Name;
            var customer = DefaultRole.Customer.GetAttributeOfType<RoleInformationAttribute>().Name;

            // client admin and customer already exists
            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, clientAdmin, It.IsAny<Guid>())).Returns(true);
            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, customer, It.IsAny<Guid>())).Returns(true);

            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, broker, It.IsAny<Guid>())).Returns(false);
            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, underWriter, It.IsAny<Guid>())).Returns(false);
            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, claimsAgent, It.IsAny<Guid>())).Returns(false);
            roleRepository.Setup(c => c.IsNameInUse(tenant.Id, productDeveloper, It.IsAny<Guid>())).Returns(false);

            roleRepository.Setup(c => c.Insert(It.IsAny<Role>()));
            roleRepository.Setup(c => c.SaveChanges());
            this.mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            var sut = new RoleService(
                roleRepository.Object,
                this.mockCachingResolver.Object,
                this.clock,
                this.mockUserSessionDeletionService.Object,
                this.mockDbContext.Object);

            // Act
            sut.CreateDefaultRolesForTenant(tenant);

            // Assert
            // verify all default client roles are added to repository and called once
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.Broker.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.UnderWriter.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.ClaimsAgent.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Once);
            var clientProductDeveloperRole = DefaultRole.ClientProductDeveloper.GetAttributeOfType<RoleInformationAttribute>();
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == clientProductDeveloperRole.Name && r.Type == clientProductDeveloperRole.RoleType)), Times.Once);

            // verify client admin and customer should not be added anymore.
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.TenantAdmin.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Never);
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.Customer.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Never);

            // verify all default MASTER roles are NOT added.
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == DefaultRole.MasterAdmin.GetAttributeOfType<RoleInformationAttribute>().Name)), Times.Never);

            var masterProductDeveloperRole = DefaultRole.MasterProductDeveloper.GetAttributeOfType<RoleInformationAttribute>();
            roleRepository.Verify(c => c.Insert(It.Is<Role>(r => r.Name == masterProductDeveloperRole.Name && r.Type == masterProductDeveloperRole.RoleType)), Times.Never);
        }

        private UserReadModel CreateUserReadModel(Tenant tenant)
        {
            var personAggregate = PersonAggregate.CreatePerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                this.performingUserId,
                this.clock.Now());
            return new UserReadModel(
                Guid.NewGuid(),
                new PersonData(personAggregate),
                default,
                null,
                this.clock.Now(),
                UserType.Client);
        }

        public class NonPermanentClientRoleTestData : IEnumerable<object[]>
        {
            private readonly Guid fooOrganisationId = Guid.NewGuid();
            private readonly Guid fooTenantId = TenantFactory.DefaultId;

            public IEnumerator<object[]> GetEnumerator()
            {
                var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
                var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
                var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
                Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
                Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
                PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
                PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);

                yield return new object[]
                {
                    RoleHelper.CreateClientBrokerRole(this.fooTenantId, this.fooOrganisationId, SystemClock.Instance.Now()),
                };
                yield return new object[]
                {
                    RoleHelper.CreateClientClaimsAgentRole(this.fooTenantId, this.fooOrganisationId, SystemClock.Instance.Now()),
                };
                yield return new object[]
                {
                    RoleHelper.CreateClientProductDeveloperRole(this.fooTenantId, this.fooOrganisationId, SystemClock.Instance.Now()),
                };
                yield return new object[]
                {
                    RoleHelper.CreateClientUnderwriterRole(this.fooTenantId, this.fooOrganisationId, SystemClock.Instance.Now()),
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public class NonPermanentMasterRoleTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { RoleHelper.CreateUBindProductDeveloperRole(SystemClock.Instance.Now()) };
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }
    }
}
