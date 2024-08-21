// <copyright file="GetPortalSignInMethodsQueryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Queries.Portal
{
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using UBind.Application.Queries.Portal;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;
    using Xunit;

    public class GetPortalSignInMethodsQueryTests
    {
        private readonly IPortalService portalService;
        private readonly Mock<ICachingResolver> cachingResolverMock;
        private readonly Mock<IBaseUrlResolver> baseUrlResolverMock;
        private readonly Mock<IPortalReadModelRepository> portalReadModelRepositoryMock;

        public GetPortalSignInMethodsQueryTests()
        {
            this.baseUrlResolverMock = new Mock<IBaseUrlResolver>();
            this.baseUrlResolverMock.Setup(s => s.GetBaseUrl(It.IsAny<Tenant>()))
                .Returns("https://test.ubind.io/");
            this.cachingResolverMock = new Mock<ICachingResolver>();
            this.portalReadModelRepositoryMock = new Mock<IPortalReadModelRepository>();
            this.portalService = new PortalService(this.cachingResolverMock.Object, this.baseUrlResolverMock.Object, this.portalReadModelRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_RemovesDuplicateLocalAccountMethods_WhenPortalOrganisationHasManagingOrganisation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portalOrganisationId = Guid.NewGuid();
            var managingOrganisationId = Guid.NewGuid();
            var portalSignInMethodReadModelRepositoryMock = new Mock<IPortalSignInMethodReadModelRepository>();
            var authenticationMethodReadModelRepositoryMock = new Mock<IAuthenticationMethodReadModelRepository>();
            var sut = new GetPortalSignInMethodsQueryHandler(
                portalSignInMethodReadModelRepositoryMock.Object,
                authenticationMethodReadModelRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.portalService);
            var authenticationMethodReadModels = new List<AuthenticationMethodReadModelSummary>();
            authenticationMethodReadModels.Add(new AuthenticationMethodReadModelSummary
            {
                Id = Guid.NewGuid(),
                OrganisationId = portalOrganisationId,
                Name = "Local Account1",
                TypeName = AuthenticationMethodType.LocalAccount.Humanize(),
            });
            authenticationMethodReadModels.Add(new AuthenticationMethodReadModelSummary
            {
                Id = Guid.NewGuid(),
                OrganisationId = managingOrganisationId,
                Name = "Local Account2",
                TypeName = AuthenticationMethodType.LocalAccount.Humanize(),
            });
            authenticationMethodReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<EntityListFilters>()))
                .Returns(authenticationMethodReadModels);
            this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new PortalReadModel
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = portalOrganisationId,
                }));
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new OrganisationReadModel
                {
                    Id = portalOrganisationId,
                }));
            portalSignInMethodReadModelRepositoryMock.Setup(s => s.GetAll(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(new List<PortalSignInMethodReadModel>());

            // Act
            var result = await sut.Handle(new GetPortalSignInMethodsQuery(tenantId, portalId), CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_AddsMissingAuthenticationMethodsToPortalSignInMethods_WhenPortalSignInMethodsDoNotExists()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portalOrganisationId = Guid.NewGuid();
            var insurerSamlLoginAuthenticationMethodId = Guid.NewGuid();
            var companyAdLoginAuthenticationMethodId = Guid.NewGuid();
            var portalSignInMethodReadModelRepositoryMock = new Mock<IPortalSignInMethodReadModelRepository>();
            var authenticationMethodReadModelRepositoryMock = new Mock<IAuthenticationMethodReadModelRepository>();
            var sut = new GetPortalSignInMethodsQueryHandler(
                portalSignInMethodReadModelRepositoryMock.Object,
                authenticationMethodReadModelRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.portalService);
            var authenticationMethodReadModels = new List<AuthenticationMethodReadModelSummary>();
            authenticationMethodReadModels.Add(new AuthenticationMethodReadModelSummary
            {
                Id = Guid.NewGuid(),
                OrganisationId = portalOrganisationId,
                Name = "Local Account",
                TypeName = AuthenticationMethodType.LocalAccount.Humanize(),
            });
            authenticationMethodReadModels.Add(new AuthenticationMethodReadModelSummary
            {
                Id = insurerSamlLoginAuthenticationMethodId,
                OrganisationId = portalOrganisationId,
                Name = "Insurer SAML Login",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
            });
            authenticationMethodReadModels.Add(new AuthenticationMethodReadModelSummary
            {
                Id = companyAdLoginAuthenticationMethodId,
                OrganisationId = portalOrganisationId,
                Name = "Company AD Login",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
            });
            authenticationMethodReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<EntityListFilters>()))
                .Returns(authenticationMethodReadModels);
            this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new PortalReadModel
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = portalOrganisationId,
                }));
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new OrganisationReadModel
                {
                    Id = portalOrganisationId,
                }));
            var portalSignInMethods = new List<PortalSignInMethodReadModel>();
            portalSignInMethods.Add(new PortalSignInMethodReadModel
            {
                AuthenticationMethodId = companyAdLoginAuthenticationMethodId,
                Name = "Company AD Login",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
                PortalId = portalId,
                IsEnabled = true,
            });
            portalSignInMethodReadModelRepositoryMock.Setup(s => s.GetAll(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(portalSignInMethods);

            // Act
            var result = await sut.Handle(new GetPortalSignInMethodsQuery(tenantId, portalId), CancellationToken.None);

            // Assert
            result.Should().HaveCount(3);
            result.Single(p => p.AuthenticationMethodId == insurerSamlLoginAuthenticationMethodId).IsEnabled.Should().BeFalse();
            result.Single(p => p.AuthenticationMethodId == companyAdLoginAuthenticationMethodId).IsEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AddsMissingSortOrderValues_WhenNotSet()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var portalId = Guid.NewGuid();
            var portalOrganisationId = Guid.NewGuid();
            var insurerSamlLoginAuthenticationMethodId = Guid.NewGuid();
            var companyAdLoginAuthenticationMethodId = Guid.NewGuid();
            var portalSignInMethodReadModelRepositoryMock = new Mock<IPortalSignInMethodReadModelRepository>();
            var authenticationMethodReadModelRepositoryMock = new Mock<IAuthenticationMethodReadModelRepository>();
            var sut = new GetPortalSignInMethodsQueryHandler(
                portalSignInMethodReadModelRepositoryMock.Object,
                authenticationMethodReadModelRepositoryMock.Object,
                this.cachingResolverMock.Object,
                this.portalService);
            var authenticationMethodReadModels = new List<AuthenticationMethodReadModelSummary>();
            authenticationMethodReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<EntityListFilters>()))
                .Returns(authenticationMethodReadModels);
            this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new PortalReadModel
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = portalOrganisationId,
                }));
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new OrganisationReadModel
                {
                    Id = portalOrganisationId,
                }));
            var portalSignInMethods = new List<PortalSignInMethodReadModel>();
            portalSignInMethods.Add(new PortalSignInMethodReadModel
            {
                AuthenticationMethodId = companyAdLoginAuthenticationMethodId,
                Name = "One",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
                PortalId = portalId,
                IsEnabled = true,
                SortOrder = -1,
            });
            portalSignInMethods.Add(new PortalSignInMethodReadModel
            {
                AuthenticationMethodId = companyAdLoginAuthenticationMethodId,
                Name = "Two",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
                PortalId = portalId,
                IsEnabled = true,
                SortOrder = 1,
            });
            portalSignInMethods.Add(new PortalSignInMethodReadModel
            {
                AuthenticationMethodId = companyAdLoginAuthenticationMethodId,
                Name = "Three",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
                PortalId = portalId,
                IsEnabled = true,
                SortOrder = 2,
            });
            portalSignInMethods.Add(new PortalSignInMethodReadModel
            {
                AuthenticationMethodId = companyAdLoginAuthenticationMethodId,
                Name = "Four",
                TypeName = AuthenticationMethodType.Saml.Humanize(),
                PortalId = portalId,
                IsEnabled = true,
                SortOrder = -1,
            });
            portalSignInMethodReadModelRepositoryMock.Setup(s => s.GetAll(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(portalSignInMethods);

            // Act
            var result = await sut.Handle(new GetPortalSignInMethodsQuery(tenantId, portalId), CancellationToken.None);

            // Assert
            result.Should().HaveCount(4);
            var sortedResults = result.OrderBy(x => x.SortOrder).ToList();
            sortedResults[0].SortOrder.Should().Be(1);
            sortedResults[0].Name.Should().Be("Two");
            sortedResults[1].SortOrder.Should().Be(2);
            sortedResults[1].Name.Should().Be("Three");
            sortedResults[2].SortOrder.Should().Be(3);
            sortedResults[2].Name.Should().Be("One");
            sortedResults[3].SortOrder.Should().Be(4);
            sortedResults[3].Name.Should().Be("Four");
        }
    }
}
