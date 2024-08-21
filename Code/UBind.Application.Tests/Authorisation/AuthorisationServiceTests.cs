// <copyright file="AuthorisationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Authorisation
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Principal;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.Services;
    using Xunit;

    public class AuthorisationServiceTests
    {
        /// <summary>
        /// Defines the currentTenant.
        /// </summary>
        private readonly Guid currentTenant = Guid.NewGuid();

        /// <summary>
        /// Defines the currentOrganisation.
        /// </summary>
        private readonly Guid currentOrganisationId = Guid.NewGuid();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid customerId = new Guid("FA449334-3333-2222-1111-000000000000");

        private IAuthorisationService authorisationService;
        private Mock<IOrganisationService> organisationService = new Mock<IOrganisationService>();
        private Mock<User.IUserService> userService = new Mock<User.IUserService>();
        private Mock<ICustomerService> customerService = new Mock<ICustomerService>();
        private Mock<IRoleService> roleService = new Mock<IRoleService>();
        private Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private Mock<IPolicyReadModelRepository> policyReadModelRepository = new Mock<IPolicyReadModelRepository>();
        private Mock<IClaimReadModelRepository> claimReadModelRepository = new Mock<IClaimReadModelRepository>();
        private Mock<IQuoteReadModelRepository> quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private Mock<IDkimSettingRepository> dkimSettingRepository = new Mock<IDkimSettingRepository>();
        private Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private Mock<IReportReadModelRepository> reportReadModelRepository = new Mock<IReportReadModelRepository>();
        private Mock<IUserSessionService> userSessionServiceMock = new Mock<IUserSessionService>();

        public AuthorisationServiceTests()
        {
            var orgReadModelSummary = new Mock<IOrganisationReadModelSummary>();
            orgReadModelSummary.Setup(o => o.TenantId).Returns(this.currentTenant);
            this.organisationService.Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(
                It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(orgReadModelSummary.Object);

            this.authorisationService = this.GetAuthorisationService();
        }

        [Fact]
        public async Task ThrowIfUserNotInOrganisationOrDefaultOrganisation_Throws_WhenUserNotInOrganisation()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ManageUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal();
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                claimsPrincipal, Guid.NewGuid(), this.currentTenant);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserNotInOrganisationOrDefaultOrganisation_DoesNotThrow_WhenUserInDefaultOrganisation()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ManageUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal();
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                claimsPrincipal, Guid.NewGuid(), this.currentTenant);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ThrowIfUserCannotModifyCustomer_Throws_WhenUserDoesntHaveManageCustomersPermission()
        {
            // Arrange
            // TODO: Mock up doing permissions check using new Query
            var permissions = new List<Permission>
            {
                Permission.ManageUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, null, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockCustomer = new Mock<ICustomerReadModelSummary>();
            this.customerService.Setup(u => u.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(mockCustomer.Object);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotModifyCustomer(
                claimsPrincipal.GetTenantId(), claimsPrincipal, Guid.NewGuid(), this.currentTenant);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewQuote_Throws_WhenCustomerTriesToAccessAnothersQuoteAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewQuotes,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Customer, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockQuote = new Mock<IQuoteReadModelSummary>();
            mockQuote.Setup(m => m.CustomerId).Returns(Guid.NewGuid());
            mockQuote.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewQuote(
                claimsPrincipal, mockQuote.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewQuote_Throws_WhenAgentTriesToAccessQuoteTheyDontOwnAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewQuotes,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockQuote = new Mock<IQuoteReadModelSummary>();
            mockQuote.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockQuote.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewQuote(
                claimsPrincipal, mockQuote.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewQuote_Throws_WhenNonTenantAdminTriesToAccessQuotesAnotherOrgAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewQuotes,
                Permission.ManageOrganisationAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockQuote = new Mock<IQuoteReadModelSummary>();
            mockQuote.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewQuote(
                claimsPrincipal, mockQuote.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewClaim_Throws_WhenCustomerTriesToAccessAnothersClaimAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewClaims,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Customer, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockClaim = new Mock<IClaimReadModelSummary>();
            mockClaim.Setup(m => m.CustomerId).Returns(Guid.NewGuid());
            mockClaim.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewClaim(
                claimsPrincipal, mockClaim.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewClaim_Throws_WhenAgentTriesToAccessClaimTheyDontOwnAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewClaims,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockClaim = new Mock<IClaimReadModelSummary>();
            mockClaim.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockClaim.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewClaim(
                claimsPrincipal, mockClaim.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewClaim_Throws_WhenNonTenantAdminTriesToAccessClaimsAnotherOrgAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewClaims,
                Permission.ManageOrganisationAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockClaim = new Mock<IClaimReadModelSummary>();
            mockClaim.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewClaim(
                claimsPrincipal, mockClaim.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewPolicy_Throws_WhenCustomerTriesToAccessAnothersPolicyAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Customer, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockPolicy = new Mock<IPolicyReadModelSummary>();
            mockPolicy.Setup(m => m.CustomerId).Returns(Guid.NewGuid());
            mockPolicy.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewPolicy(
                claimsPrincipal, mockPolicy.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewPolicy_Throws_WhenAgentTriesToAccessPolicyTheyDontOwnAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockPolicy = new Mock<IPolicyReadModelSummary>();
            mockPolicy.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockPolicy.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewPolicy(
                claimsPrincipal, mockPolicy.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewPolicy_Throws_WhenNonTenantAdminTriesToAccessPolicyFromAnotherOrgAsync()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
                Permission.ManageOrganisationAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockPolicy = new Mock<IPolicyReadModelSummary>();
            mockPolicy.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = async () => await this.authorisationService.ThrowIfUserCannotViewPolicy(
                claimsPrincipal, mockPolicy.Object);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewCustomer_Throws_WhenAgentTriesToAccessCustomerTheyDontOwn()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockCustomer = new Mock<ICustomerReadModelSummary>();
            mockCustomer.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockCustomer.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotViewCustomer(
                claimsPrincipal, mockCustomer.Object);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewCustomer_Throws_WhenTenantAdminTriesToAccessCustomerFromAnotherOrg()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
                Permission.ManageOrganisationAdminUsers,
                Permission.ManageTenantAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockCustomer = new Mock<ICustomerReadModelSummary>();
            mockCustomer.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotViewCustomer(
                claimsPrincipal, mockCustomer.Object);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewEmail_Throws_WhenCustomerTriesToAccessAnotherCustomersEmail()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewMessages,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Customer, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockEmail = new Mock<IEmailDetails>();
            mockEmail.Setup(m => m.Customer).Returns(new CustomerData { Id = Guid.NewGuid() });
            mockEmail.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            var mockCustomer = new Mock<ICustomerReadModelSummary>();
            mockCustomer.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockCustomer.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.customerService.Setup(c => c.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockCustomer.Object);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotViewEmail(
                claimsPrincipal, mockEmail.Object);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewEmail_Throws_WhenAgentTriesToAccessEmailTheyDontOwn()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewMessages,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var mockEmail = new Mock<IEmailDetails>();
            mockEmail.Setup(m => m.Customer).Returns(new CustomerData { Id = Guid.NewGuid() });
            mockEmail.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            var mockCustomer = new Mock<ICustomerReadModelSummary>();
            mockCustomer.Setup(m => m.OwnerUserId).Returns(Guid.NewGuid());
            mockCustomer.Setup(m => m.OrganisationId).Returns(this.currentOrganisationId);
            this.customerService.Setup(c => c.GetCustomerById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(mockCustomer.Object);
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotViewEmail(
                claimsPrincipal, mockEmail.Object);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewEmail_Throws_WhenNonTenantAdminTriesToAccessEmailsInAnotherOrg()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewMessages,
                Permission.ManageOrganisationAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            var mockEmail = new Mock<IEmailDetails>();
            mockEmail.Setup(m => m.OrganisationId).Returns(Guid.NewGuid());
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            Func<Task> act = () => this.authorisationService.ThrowIfUserCannotViewEmail(
                claimsPrincipal, mockEmail.Object);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>()).And
                .Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ApplyRestrictionsToFilters_SetsOrgIdToPerformingUsersOrg_WhenAgentUserFilteringQuotes()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                Permission.ManageOrganisationAdminUsers,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var filters = new EntityListFilters();
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            await this.authorisationService.ApplyViewPolicyRestrictionsToFilters(
                claimsPrincipal, filters);

            // Assert
            filters.OrganisationIds.Should().HaveCount(1);
            filters.OrganisationIds.Should().Contain(this.currentOrganisationId);
        }

        [Fact]
        public async Task ApplyRestrictionsToFilters_RestrictsAgentsToSeeingTheirOwnThings()
        {
            // Arrange
            var permissions = new List<Permission>
            {
                Permission.ViewPolicies,
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, UserType.Client, null, null, null);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var filters = new EntityListFilters();
            this.mediator.Setup(s => s.Send(It.IsAny<PrincipalHasPermissionQuery>(), It.IsAny<CancellationToken>()))
                .Returns<PrincipalHasPermissionQuery, CancellationToken>((query, token) =>
                {
                    return Task.FromResult(permissions.Contains(query.Permission));
                });
            this.mediator.Setup(s => s.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(new UserAuthenticationData(
                        claimsPrincipal.GetTenantId(),
                        claimsPrincipal.GetOrganisationId(),
                        claimsPrincipal.GetUserType(),
                        claimsPrincipal.GetId().Value,
                        claimsPrincipal.GetCustomerId(),
                        permissions) as IUserAuthenticationData));

            // Act
            await this.authorisationService.ApplyRestrictionsToFilters(claimsPrincipal, filters);

            // Assert
            filters.OwnerUserId.Should().Be(this.userId);
        }

        private ClaimsPrincipal GetClaimsPrincipal(
            Guid? userId = null,
            UserType? userType = null,
            Guid? tenantId = null,
            Guid? organisationId = null,
            Guid? customerId = null)
        {
            userId = userId ?? this.userId;
            userType = userType ?? UserType.Client;
            tenantId = tenantId ?? this.currentTenant;
            organisationId = organisationId ?? this.currentOrganisationId;
            customerId = customerId ?? this.customerId;
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, userType.Humanize()),
                new Claim("Tenant", tenantId.ToString()),
                new Claim("OrganisationId", organisationId.ToString()),
                new Claim("CustomerId", customerId.ToString()),
            }));
        }

        private AuthorisationService GetAuthorisationService()
        {
            var authorisationService = new AuthorisationService(
            this.organisationService.Object,
            this.userService.Object,
            this.customerService.Object,
            this.roleService.Object,
            this.policyReadModelRepository.Object,
            this.claimReadModelRepository.Object,
            this.quoteReadModelRepository.Object,
            this.mediator.Object,
            this.cachingResolver.Object,
            this.quoteAggregateResolverService.Object,
            this.dkimSettingRepository.Object,
            this.httpContextPropertiesResolver.Object,
            this.reportReadModelRepository.Object,
            this.userSessionServiceMock.Object);
            return authorisationService;
        }
    }
}
