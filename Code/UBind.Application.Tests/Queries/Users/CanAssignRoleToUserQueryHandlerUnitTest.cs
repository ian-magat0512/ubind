// <copyright file="CanAssignRoleToUserQueryHandlerUnitTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Users.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Application.Queries.Role;
    using UBind.Application.Queries.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.Tests.Helpers;
    using Xunit;

    public class CanAssignRoleToUserQueryHandlerUnitTest
    {
        public CanAssignRoleToUserQueryHandlerUnitTest()
        {
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Theory]
        [InlineData("Master", "Master", true)]
        [InlineData("Client", "Client", true)]
        [InlineData("Customer", "Customer", true)]
        [InlineData("Master", "Client", false)]
        [InlineData("Master", "Customer", false)]
        [InlineData("Client", "Master", false)]
        [InlineData("Client", "Customer", false)]
        [InlineData("Customer", "Master", false)]
        [InlineData("Customer", "Client", false)]
        public async Task UserService_CanAssignSuccessful_IfRoleMatchUserTypeWithUser(string userType, string roleUserType, bool expectedResult)
        {
            // Assign
            var mockUser = new Mock<IUserReadModelSummary>();
            Mock<ICqrsMediator> mockMediatr = new Mock<ICqrsMediator>();
            Mock<IAuthorisationService> mockAuthorisationService = new Mock<IAuthorisationService>();
            Role role = null;
            switch (roleUserType)
            {
                case "Master":
                    role = this.SetupMasterAdminRole();
                    break;
                case "Client":
                    role = this.SetupTenantAdminRole();
                    break;
                case "Customer":
                    role = this.SetupCustomerRole();
                    break;
                default:
                    break;
            }

            mockUser.Setup(x => x.UserType).Returns(userType);
            IReadOnlyList<Role> roles = new List<Role> { role };
            mockMediatr.Setup(x => x.Send(It.IsAny<GetAssignableRolesMatchingFiltersQuery>(), System.Threading.CancellationToken.None))
                .Returns(Task.FromResult(roles));
            CanAssignRoleToUserQuery command = new CanAssignRoleToUserQuery(Guid.NewGuid(), Guid.NewGuid(), role, mockUser.Object);
            CanAssignRoleToUserQueryHandler handler = new CanAssignRoleToUserQueryHandler(
                mockAuthorisationService.Object, mockMediatr.Object);

            // Act
            var canAssign = await handler.Handle(command, cancellationToken: System.Threading.CancellationToken.None);

            // Assert
            canAssign.Should().Be(expectedResult);
        }

        private Role SetupMasterAdminRole()
        {
            var role = RoleHelper.CreateMasterAdminRole(default);
            return role;
        }

        private Role SetupTenantAdminRole()
        {
            var tenant = this.SetupTenant();
            var role = RoleHelper.CreateTenantAdminRole(tenant.Id, tenant.Details.DefaultOrganisationId, default);

            return role;
        }

        private Role SetupCustomerRole()
        {
            var tenant = this.SetupTenant();
            var role = RoleHelper.CreateCustomerRole(tenant.Id, tenant.Details.DefaultOrganisationId, default);

            return role;
        }

        private Domain.Tenant SetupTenant()
        {
            var tenant = TenantFactory.Create(Guid.NewGuid());
            tenant.SetDefaultOrganisation(Guid.NewGuid(), SystemClock.Instance.GetCurrentInstant());
            return tenant;
        }
    }
}
