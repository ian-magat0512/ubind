// <copyright file="ClaimServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for Claim Service.
    /// </summary>
    public class ClaimServiceTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        /// <summary>
        /// Defines the Clock.
        /// </summary>
        private readonly TestClock clock = new TestClock();

        private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();

        private QuoteAggregate quoteAggregate;

        private QuoteAggregate existingPolicy;

        private ClaimAggregate claimAggregate;

        private CustomerAggregate customerAggregate;

        private Mock<IPolicyReadModelDetails> policyDetails;

        private PersonAggregate personAggregate;

        private ClaimService claimService;

        public ClaimServiceTests()
        {
            this.quoteAggregate = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            this.existingPolicy = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            this.quoteAggregateRepository.Setup(p => p.GetById(this.quoteAggregate.TenantId, this.existingPolicy.Policy.PolicyId)).Returns(this.quoteAggregate);

            var customerImportedData = new CustomerImportData();
            var tenant = TenantFactory.Create();

            this.personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, customerImportedData, this.performingUserId, this.clock.Now());
            this.customerAggregate = CustomerAggregate.CreateImportedCustomer(
                this.personAggregate.TenantId,
                this.personAggregate,
                DeploymentEnvironment.Development,
                this.performingUserId,
                null,
                this.clock.Now());
            this.claimAggregate = ClaimAggregate.CreateForPolicy(
               "AAAAAA",
               this.quoteAggregate,
               Guid.NewGuid(),
               "John Smith",
               "Jonboy",
               this.performingUserId,
               this.clock.Now());

            this.policyDetails = new Mock<IPolicyReadModelDetails>();
            this.policyDetails.Setup(p => p.TenantId).Returns(TenantFactory.DefaultId);
            this.claimService = this.GetClaimService(this.policyDetails);
        }

        /// <summary>
        /// Test associate claim with policy succeed.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task AssociateClaimWithPolicyAysnc_SetsCorrectPolicyIdOnClaim_WhenSuccessful()
        {
            // Arrange
            var existingPolicy = this.existingPolicy;
            var tenant = TenantFactory.Create();
            this.quoteAggregateRepository.Setup(p => p.GetById(tenant.Id, this.existingPolicy.Policy.PolicyId)).Returns(this.existingPolicy);

            // Act
            var updatedClaim = await this.claimService.AssociateClaimWithPolicyAsync(tenant.Id, existingPolicy.Policy.PolicyId, this.claimAggregate.Id);

            // Assert
            updatedClaim.PolicyId.Should().Be(existingPolicy.Policy.PolicyId);
        }

        /// <summary>
        /// Test associate claim with policy throw error exception.
        /// </summary>
        [Fact]
        public void AssociateClaimWithPolicyPolicyAsync_ThrowsErrorException_WhenAssociatingTheSamePolicy()
        {
            // Arrange
            var policyDetails = new Mock<IPolicyReadModelDetails>();
            var claimService = this.GetClaimService(policyDetails);

            var tenant = TenantFactory.Create();

            // Act
            Func<Task> act = async () => await claimService.AssociateClaimWithPolicyAsync(tenant.Id, policyDetails.Object.PolicyId, this.claimAggregate.Id);

            // Assert
            act.Should().ThrowAsync<ErrorException>();
        }

        /// <summary>
        /// Test associate to non existing policy throw error exception.
        /// </summary>
        [Fact]
        public void AssociateClaimWithPolicyAsync_ThrowErrorException_WhenPolicyDoesNotExist()
        {
            // Arrange
            var nonExistingPolicy = Guid.NewGuid();
            var policyDetails = new Mock<IPolicyReadModelDetails>();
            var claimService = this.GetClaimService(policyDetails);

            var tenant = TenantFactory.Create();

            // Act
            Func<Task> act = async () => await claimService.AssociateClaimWithPolicyAsync(tenant.Id, policyDetails.Object.PolicyId, this.claimAggregate.Id);

            // Assert
            act.Should().ThrowAsync<ErrorException>();
        }

        private ClaimService GetClaimService(Mock<IPolicyReadModelDetails> policyDetails)
        {
            var policyReadModelRepository = new Mock<IPolicyReadModelRepository>();
            var claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            var customerAggregateRepository = new Mock<ICustomerAggregateRepository>();
            var secondPolicy = new Mock<IPolicyReadModelDetails>();
            var claimAggregateRepository = new Mock<IClaimAggregateRepository>();
            var quoteAggregate = QuoteFactory.CreateNewPolicy(TenantFactory.DefaultId);
            var additionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>();
            var mediator = new Mock<ICqrsMediator>();
            var personAggregateRepository = new Mock<IPersonAggregateRepository>();
            personAggregateRepository.Setup(p => p.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(this.personAggregate);
            customerAggregateRepository.Setup(p => p.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(this.customerAggregate);

            claimAggregateRepository.Setup(p => p.GetById(this.claimAggregate.TenantId, this.claimAggregate.Id)).Returns(this.claimAggregate);
            var claim = new Mock<IClaimReadModelSummary>();
            claim.Setup(e => e.Id).Returns(this.claimAggregate.Id);
            claim.Setup(e => e.PolicyId).Returns(quoteAggregate.Policy.PolicyId);
            claim.Setup(e => e.Environment).Returns(DeploymentEnvironment.Staging);

            policyReadModelRepository.Setup(p => p.GetPolicyDetails(TenantFactory.DefaultId, policyDetails.Object.PolicyId)).Returns(policyDetails.Object);
            secondPolicy.Setup(t => t.TenantId).Returns(TenantFactory.DefaultId);
            if (secondPolicy.Object.CustomerId.HasValue)
            {
                customerAggregateRepository.Setup(p => p.GetById(TenantFactory.DefaultId, secondPolicy.Object.CustomerId.Value)).Returns(this.customerAggregate);
            }

            claimReadModelRepository.Setup(p => p.GetSummaryById(TenantFactory.DefaultId, this.claimAggregate.Id)).Returns(claim.Object);

            this.quoteAggregateRepository.Setup(e => e.GetById(TenantFactory.DefaultId, quoteAggregate.Policy.PolicyId)).Returns(quoteAggregate);

            var claimService = new ClaimService(
                new Mock<IClaimWorkflowProvider>().Object,
                claimAggregateRepository.Object,
                claimReadModelRepository.Object,
                this.quoteAggregateRepository.Object,
                customerAggregateRepository.Object,
                personAggregateRepository.Object,
                new Mock<IClaimNumberRepository>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                new Mock<ISystemAlertService>().Object,
                new Mock<ICachingResolver>().Object,
                this.clock);
            return claimService;
        }
    }
}
