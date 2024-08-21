// <copyright file="DropGenerationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using eWAY.Rapid.Enums;
    using eWAY.Rapid.Models;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Extensions;
    using Stripe;
    using UBind.Application.Payment.Deft;
    using UBind.Application.Report;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Report;
    using UBind.Domain.Configuration;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Report;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    public class DropGenerationServiceTests
    {
        private readonly Mock<IPolicyTransactionReadModelRepository> policyTransactionRepository;
        private readonly Mock<IQuoteReadModelRepository> quoteReadModelRepository;
        private readonly Mock<IEmailRepository> emailRepository;
        private readonly Mock<IOrganisationService> organisationService;
        private readonly Mock<IEmailInvitationConfiguration> configuration;
        private readonly Mock<IClaimReadModelRepository> claimReadModelRepository;
        private readonly Guid? performingUserId;

        public DropGenerationServiceTests()
        {
            this.policyTransactionRepository = new Mock<IPolicyTransactionReadModelRepository>();
            this.quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
            this.emailRepository = new Mock<IEmailRepository>();
            this.organisationService = new Mock<IOrganisationService>();
            this.configuration = new Mock<IEmailInvitationConfiguration>();
            this.claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            this.performingUserId = Guid.NewGuid();
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnPolicyTransactionAndQuoteAndEmailData_WhenSourceDataIncludesEmailAndQuoteAndPolicyTransactions()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                    ReportSourceDataType.Quote,
                    ReportSourceDataType.ProductEmail,
                    ReportSourceDataType.SystemEmail,
                    ReportSourceDataType.Claim,
                };
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };

            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            listPolicyTransactions.Add(new FakePolicyTransactionReadModel("P0001", SystemClock.Instance.Now()));

            var quoteItems = new List<IQuoteReportItem>();
            quoteItems.Add(new FakeQuoteReportItem("Q0001", SystemClock.Instance.Now()));

            var productEmailItems = new List<IEmailDetails>();
            productEmailItems.Add(new FakeEmailReportItem("Product Email Subject", fromDate));

            var systemEmailItems = new List<IEmailDetails>();
            systemEmailItems.Add(new FakeEmailReportItem("System Email Subject", fromDate));

            var claimItems = new List<IClaimReportItem>();
            claimItems.Add(new FakeClaimReportItem() { ClaimNumber = "C0001", CreatedTimestamp = fromDate, LastModifiedTimestamp = fromDate });
            claimItems.Add(new FakeClaimReportItem() { ClaimNumber = "C0002", CreatedTimestamp = fromDate, LastModifiedTimestamp = fromDate });

            this.policyTransactionRepository
                .Setup(r => r.GetPolicyTransactions(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<TransactionType>>()))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(quoteItems);
            this.emailRepository
                .Setup(r => r.GetProductEmailForReport(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(productEmailItems);
            this.emailRepository
                .Setup(r => r.GetSystemEmailForReport(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>()))
               .Returns(systemEmailItems);
            this.claimReadModelRepository
                 .Setup(r => r.GetClaimsDataForReports(
                     It.IsAny<Guid>(),
                     It.IsAny<Guid>(),
                     It.IsAny<IEnumerable<Guid>>(),
                     It.IsAny<DeploymentEnvironment>(),
                     It.IsAny<Instant>(),
                     It.IsAny<Instant>(),
                     It.IsAny<bool>()))
                .Returns(claimItems);

            var dropCreationService = new DropGenerationService(
                this.policyTransactionRepository.Object,
                this.quoteReadModelRepository.Object,
                this.emailRepository.Object,
                this.configuration.Object,
                this.organisationService.Object,
                this.claimReadModelRepository.Object);

            // Act
            var output = await dropCreationService.GenerateReportDrop(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                productIds,
                env,
                sourceData,
                fromDate,
                toDate,
                false);

            // Assert
            output.PolicyTransactions.Should().Contain(p => p.PolicyNumber == "P0001");
            output.Quotes.Should().Contain(q => q.QuoteReference == "Q0001");
            output.Emails.Should().Contain(e => e.Subject == "Product Email Subject" || e.Subject == "System Email Subject");
            output.Claims.Should().Contain(c => c.ClaimNumber == "C0001" || c.ClaimNumber == "C0002");
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnPolicyTransactionOnly_WhenSourceDataIncludesPolicyTransactionItemsOnly()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null, this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                };
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };

            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            listPolicyTransactions.Add(new FakePolicyTransactionReadModel("P0001", SystemClock.Instance.Now()));

            var quoteItems = new List<IQuoteReportItem>();
            quoteItems.Add(new FakeQuoteReportItem("Q0001", SystemClock.Instance.Now()));

            this.policyTransactionRepository
                .Setup(r => r.GetPolicyTransactions(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<TransactionType>>()))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(quoteItems);

            var dropCreationService = new DropGenerationService(
                this.policyTransactionRepository.Object,
                this.quoteReadModelRepository.Object,
                this.emailRepository.Object,
                this.configuration.Object,
                this.organisationService.Object,
                this.claimReadModelRepository.Object);

            // Act
            var output = await dropCreationService.GenerateReportDrop(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                productIds,
                env,
                sourceData,
                fromDate,
                toDate,
                false);

            // Assert
            output.PolicyTransactions.Should().Contain(p => p.PolicyNumber == "P0001");
            output.Quotes.Should().NotContain(q => q.QuoteReference == "Q0001");
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnQuoteDataOnly_WhenSourceDataIncludesQuoteOnly()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.Quote,
                };
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };

            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            listPolicyTransactions.Add(new FakePolicyTransactionReadModel("P0001", SystemClock.Instance.Now()));

            var quoteItems = new List<IQuoteReportItem>();
            quoteItems.Add(new FakeQuoteReportItem("Q0001", SystemClock.Instance.Now()));

            this.policyTransactionRepository
                .Setup(r => r.GetPolicyTransactions(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<TransactionType>>()))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(quoteItems);

            var dropCreationService = new DropGenerationService(
               this.policyTransactionRepository.Object,
               this.quoteReadModelRepository.Object,
               this.emailRepository.Object,
               this.configuration.Object,
               this.organisationService.Object,
               this.claimReadModelRepository.Object);

            // Act
            var output = await dropCreationService.GenerateReportDrop(
                tenant.Id, tenant.Details.DefaultOrganisationId, productIds, env, sourceData, fromDate, toDate, false);

            // Assert
            output.PolicyTransactions.Should().NotContain(p => p.PolicyNumber == "P0001");
            output.Quotes.Should().Contain(q => q.QuoteReference == "Q0001");
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnQuoteDataOnly_ReportAggregateBodyTemplate_ShouldMatchQuote()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.Quote,
                };
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };

            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            listPolicyTransactions.Add(new FakePolicyTransactionReadModel("P0001", SystemClock.Instance.Now()));

            var quoteItems = new List<IQuoteReportItem>();
            quoteItems.Add(new FakeQuoteReportItem("Q0001", SystemClock.Instance.Now()));

            this.policyTransactionRepository
                .Setup(r => r.GetPolicyTransactions(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<TransactionType>>()))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(quoteItems);

            var dropCreationService = new DropGenerationService(
               this.policyTransactionRepository.Object,
               this.quoteReadModelRepository.Object,
               this.emailRepository.Object,
               this.configuration.Object,
               this.organisationService.Object,
               this.claimReadModelRepository.Object);

            var report = new ReportAddModel
            {
                Name = "Report Name",
                Description = "Report Description",
                ProductIds = productIds.ToList(),
                SourceData = "New Business,Renewal,Adjustment,Cancellation,Quote,System Email,Product Email,Claim",
                MimeType = "text/plain",
                Filename = "filename.txt",
                Body = "TransactionType,ProductName,CreationDate,CreatedTimestamp,InceptionDate,InceptionTime,AdjustmentDate,AdjustmentTime,PolicyNumber,QuoteReference,InvoiceNumber,CreditNoteNumber,CustomerFullName,CustomerEmail,RatingState,BasePremium,Esl,PremiumGst,StampDutyAct,StampDutyNsw,StampDutyNt,StampDutyQld,StampDutySa,StampDutyTas,StampDutyVic,StampDutyWa,StampDutyTotal,TotalPremium,Commission,CommissionGst,BrokerFee,BrokerFeeGst,UnderwriterFee,UnderwriterFeeGst,TotalGst,TotalPayable {% for Quote in Quotes %}\"",
            };

            // Act
            var reportAggregate = ReportAggregate.CreateReport(tenant.Id, organisation.Id, report, this.performingUserId, SystemClock.Instance.Now());
            var output = await dropCreationService.GenerateReportDrop(
                tenant.Id, tenant.Details.DefaultOrganisationId, productIds, env, sourceData, fromDate, toDate, false);

            // Assert
            reportAggregate.Body.Should().ContainAny(new string[] { "Quote", "Quotes" });
            output.Quotes.Should().Contain(q => q.QuoteReference == "Q0001");
        }

        [Fact]
        public async Task GenerateReportDrop_ShouldNotThrowException_WhenSourceDataContainsNonPolicyTransactionType()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                    ReportSourceDataType.Quote,
                };
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };
            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            var quoteItems = new List<IQuoteReportItem>();

            this.policyTransactionRepository
                .Setup(r => r.GetPolicyTransactions(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<TransactionType>>()))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DeploymentEnvironment>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>(),
                    It.IsAny<bool>()))
                .Returns(quoteItems);

            var dropCreationService = new DropGenerationService(
                this.policyTransactionRepository.Object,
                this.quoteReadModelRepository.Object,
                this.emailRepository.Object,
                this.configuration.Object,
                this.organisationService.Object,
                this.claimReadModelRepository.Object);

            // Act
            var exception = await Record.ExceptionAsync(() => dropCreationService.GenerateReportDrop(
                tenant.Id, tenant.Details.DefaultOrganisationId, productIds, env, sourceData, fromDate, toDate, false));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnPolicyTransactionWithDeftPaymentInfo_WhenPolicyTransaction_Has_DeftPaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentResponse = @"{
                                ""ResponseTimestamp"": ""11112020150806"",
                                ""SettlementDate"": ""2020-11-11"",
                                ""Drn"": ""637407040862870110"",
                                ""Crn"": ""8391500000"",
                                ""ReceiptNumber"": ""637407040865050118"",
                                ""SurchargeRate"": 5.4,
                                ""Fee"": 637407040863940100.0,
                                ""SurchargeAmount"": 0.0,
                                ""TotalAmount"": 365.25,
                                ""SecureToken"": """"
                            }";

            var paymentDetails = CreateDeftPaymentEvent(paymentResponse);
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentResponse, nameof(PaymentGatewayName.Deft));

            // Assert
            var policy = output.PolicyTransactions.FirstOrDefault(p => p.PolicyNumber == "P0001")!;
            policy.DeftPaymentResponse.Should().NotBeNull();
            policy.DeftPaymentResponse.Crn.Should().Be("8391500000");
            policy.DeftPaymentResponse.Drn.Should().Be("637407040862870110");
            policy.DeftPaymentResponse.ReceiptNumber.Should().Be("637407040865050118");
            policy.DeftPaymentResponse.ResponseTimestamp.Should().Be("11112020150806");
            policy.DeftPaymentResponse.SettlementDate.Should().Be("2020-11-11");
            policy.DeftPaymentResponse.TotalAmount.Should().Be(365.25M);
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnPolicyTransaction_No_DeftPaymentInfo_WhenPolicyTransaction_Has_EwayPaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentDetails = CreateEwayPaymentEvent();
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentDetails, nameof(PaymentGatewayName.EWay));

            // Assert
            var policy = output.PolicyTransactions.FirstOrDefault(p => p.PolicyNumber == "P0001");
            policy?.DeftPaymentResponse.Should().BeNull();
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnPolicyTransaction_No_DeftPaymentInfo_WhenPolicyTransaction_Has_StripePaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentDetails = CreateStripePaymentEvent();
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.NewBusiness,
                    ReportSourceDataType.Renewal,
                    ReportSourceDataType.Adjustment,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentDetails, nameof(PaymentGatewayName.Stripe));

            // Assert
            var policy = output.PolicyTransactions.FirstOrDefault(p => p.PolicyNumber == "P0001");
            policy?.DeftPaymentResponse.Should().BeNull();
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnQuoteWithDeftPaymentInfo_WhenPolicyTransaction_Has_DeftPaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentDetails = CreateDeftPaymentEvent(null);
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.Quote,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentDetails, nameof(PaymentGatewayName.Deft));

            // Assert
            var quote = output.Quotes.FirstOrDefault(q => q.QuoteReference == "Q0001")!;
            quote?.DeftPaymentResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnQuote_No_DeftPaymentInfo_WhenPolicyTransaction_Has_StripePaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentDetails = CreateStripePaymentEvent();
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.Quote,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentDetails, nameof(PaymentGatewayName.Stripe));

            // Assert
            var quote = output.Quotes.FirstOrDefault(q => q.QuoteReference == "Q0001");
            quote?.DeftPaymentResponse.Should().BeNull();
        }

        [Fact]
        public async Task GenerateReportDrop_ReturnQuote_No_DeftPaymentInfo_WhenPolicyTransaction_Has_EwayPaymentMade()
        {
            // Arrange
            var tenant = TenantFactory.Create();
            var organisation = Domain.Aggregates.Organisation.Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            tenant.SetDefaultOrganisation(organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromSeconds(1)));

            this.organisationService
                .Setup(t => t.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);

            var paymentDetails = CreateEwayPaymentEvent();
            IEnumerable<ReportSourceDataType> sourceData =
                new ReportSourceDataType[]
                {
                    ReportSourceDataType.Quote,
                };

            // Act
            var output = await this.GenerateReport(sourceData, paymentDetails, nameof(PaymentGatewayName.EWay));

            // Assert
            var quote = output.Quotes.FirstOrDefault(q => q.QuoteReference == "Q0001");
            quote?.DeftPaymentResponse.Should().BeNull();
        }

        private static string CreateDeftPaymentEvent(string? paymentResponse)
        {
            var performingUserId = Guid.NewGuid();
            var creditCardDetails = new CreditCardDetails("4444333322221111", "John Smith", "12", 2020, "123");
            var request = new DeftPaymentRequest(
                new TestDeftConfiguration(),
                PriceBreakdown.Zero(PriceBreakdown.DefaultCurrencyCode),
                creditCardDetails,
                "123123123",
                string.Empty,
                string.Empty,
                "123123",
                "VISA",
                SystemClock.Instance.GetCurrentInstant());
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var quoteAggregate = quote.Aggregate
                        .WithCustomerDetails(quote.Id)
                        .WithFormData(quote.Id)
                        .WithCalculationResult(quote.Id)
                        .WithCustomer()
                        .WithQuoteNumber(quote.Id);

            var paymentDetails = new Domain.Aggregates.Quote.PaymentDetails(PaymentGatewayName.Deft, decimal.Zero, string.Empty, JsonConvert.SerializeObject(request), paymentResponse!);
            var paymentMade = new PaymentMadeEvent(quote.Aggregate.TenantId, quote.Id, quote, paymentDetails, performingUserId, SystemClock.Instance.Now());
            return JsonConvert.SerializeObject(paymentMade);
        }

        private static string CreateStripePaymentEvent()
        {
            var options = new ChargeCreateOptions
            {
                Amount = (int)(12345 * 100),
                Currency = "aud",
                Description = "xxxxxxx",
                Source = "1234567890", // obtained with Stripe.js,
            };
            var capturedPayload = new
            {
                options.Amount,
                options.Currency,
                options.Description,
                options.Source,
            };
            var performingUserId = Guid.NewGuid();
            var requestJson = JsonConvert.SerializeObject(capturedPayload);
            var response = @"{
                                ""id"": ""ch_1HHLXCCHkDPdUdSJqgcIqT00"",
                                ""object"": ""charge"",
                                ""amount"": 100815,
                                ""amount_refunded"": 0,
                                ""application"": null,
                                ""application_fee"": null,
                                ""application_fee_amount"": null,
                                ""balance_transaction"": ""txn_1HHLXCCHkDPdUdSJ5kpuPuJh"",
                                ""billing_details"": {
                                    ""address"": {
                                        ""city"": null,
                                        ""country"": null,
                                        ""line1"": null,
                                        ""line2"": null,
                                        ""postal_code"": null,
                                        ""state"": null
                                    },
                                    ""email"": null,
                                    ""name"": ""Test"",
                                    ""phone"": null
                                },
                                ""captured"": true,
                                ""created"": 1597721450,
                                ""currency"": ""aud"",
                                ""customer"": null,
                                ""description"": ""GUINKL / Jomar Yanez / eddcdb2e-ad7a-4f65-b23a-257573451f6a"",
                                ""destination"": null,
                                ""dispute"": null,
                                ""failure_code"": null,
                                ""failure_message"": null,
                                ""fraud_details"": {},
                                ""invoice"": null,
                                ""livemode"": false,
                                ""metadata"": {},
                                ""on_behalf_of"": null,
                                ""order"": null,
                                ""level3"": null,
                                ""outcome"": {
                                    ""network_status"": ""approved_by_network"",
                                    ""reason"": null,
                                    ""risk_level"": ""normal"",
                                    ""risk_score"": 27,
                                    ""rule"": null,
                                    ""seller_message"": ""Payment complete."",
                                    ""type"": ""authorized""
                                },
                                ""paid"": true,
                                ""payment_intent"": null,
                                ""payment_method"": ""card_1HHLX9CHkDPdUdSJhusgXaWq"",
                                ""payment_method_details"": {
                                    ""ach_credit_transfer"": null,
                                    ""ach_debit"": null,
                                    ""alipay"": null,
                                    ""bancontact"": null,
                                    ""bitcoin"": null,
                                    ""card"": {
                                        ""brand"": ""visa"",
                                        ""checks"": {
                                            ""address_line1_check"": null,
                                            ""address_postal_code_check"": null,
                                            ""cvc_check"": ""pass""
                                        },
                                        ""country"": ""US"",
                                        ""exp_month"": 10,
                                        ""exp_year"": 2020,
                                        ""fingerprint"": ""9xfLZ4nS6iMVPPg3"",
                                        ""funding"": ""credit"",
                                        ""last4"": ""4242"",
                                        ""three_d_secure"": null,
                                        ""wallet"": null
                                    },
                                    ""card_present"": null,
                                    ""eps"": null,
                                    ""giropay"": null,
                                    ""ideal"": null,
                                    ""multibanco"": null,
                                    ""p24"": null,
                                    ""sepa_debit"": null,
                                    ""stripe_account"": null,
                                    ""type"": ""card"",
                                    ""wechat"": null
                                },
                                ""receipt_email"": null,
                                ""receipt_number"": null,
                                ""receipt_url"": ""https://pay.stripe.com/receipts/acct_1BBz54CHkDPdUdSJ/ch_1HHLXCCHkDPdUdSJqgcIqT00/rcpt_Hr3eqFCtF3akhCHDu2TkykB1E7bsaiP"",
                                ""refunded"": false,
                                ""refunds"": {
                                    ""object"": ""list"",
                                    ""data"": [],
                                    ""has_more"": false,
                                    ""url"": ""/v1/charges/ch_1HHLXCCHkDPdUdSJqgcIqT00/refunds""
                                },
                                ""review"": null,
                                ""shipping"": null,
                                ""source"": {
                                    ""id"": ""card_1HHLX9CHkDPdUdSJhusgXaWq"",
                                    ""object"": ""card"",
                                    ""account"": null,
                                    ""address_city"": null,
                                    ""address_country"": null,
                                    ""address_line1"": null,
                                    ""address_line1_check"": null,
                                    ""address_line2"": null,
                                    ""address_state"": null,
                                    ""address_zip"": null,
                                    ""address_zip_check"": null,
                                    ""available_payout_methods"": null,
                                    ""brand"": ""Visa"",
                                    ""country"": ""US"",
                                    ""currency"": null,
                                    ""customer"": null,
                                    ""cvc_check"": ""pass"",
                                    ""default_for_currency"": false,
                                    ""dynamic_last4"": null,
                                    ""exp_month"": 10,
                                    ""exp_year"": 2020,
                                    ""fingerprint"": ""9xfLZ4nS6iMVPPg3"",
                                    ""funding"": ""credit"",
                                    ""last4"": ""4242"",
                                    ""metadata"": {},
                                    ""name"": ""Test"",
                                    ""recipient"": null,
                                    ""three_d_secure"": null,
                                    ""tokenization_method"": null,
                                    ""description"": null,
                                    ""iin"": null,
                                    ""issuer"": null
                                },
                                ""source_transfer"": null,
                                ""statement_descriptor"": null,
                                ""status"": ""succeeded"",
                                ""transfer"": null,
                                ""transfer_data"": null,
                                ""transfer_group"": null,
                                ""authorization_code"": null
                            }";
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var quoteAggregate = quote.Aggregate
                        .WithCustomerDetails(quote.Id)
                        .WithFormData(quote.Id)
                        .WithCalculationResult(quote.Id)
                        .WithCustomer()
                        .WithQuoteNumber(quote.Id);

            var paymentDetails = new Domain.Aggregates.Quote.PaymentDetails(PaymentGatewayName.Stripe, ((decimal)options.Amount) / 100m, options.Description, requestJson, response);
            var paymentMade = new PaymentMadeEvent(quote.Aggregate.TenantId, quote.Id, quote, paymentDetails, performingUserId, SystemClock.Instance.Now());
            return JsonConvert.SerializeObject(paymentMade);
        }

        private static string CreateEwayPaymentEvent()
        {
            var transaction = new Transaction
            {
                Customer = new eWAY.Rapid.Models.Customer
                {
                    CardDetails = new CardDetails
                    {
                        Name = "John Smith",
                        Number = "4444333322221111",
                        ExpiryMonth = "12",
                        ExpiryYear = "2020",
                        CVN = "123",
                    },
                },
                PaymentDetails = new eWAY.Rapid.Models.PaymentDetails
                {
                    InvoiceReference = "xxxxxxxx",
                    TotalAmount = 1245,
                },
                TransactionType = TransactionTypes.Purchase,
            };
            var response = @"{
                            ""Transaction"": {
                                ""TransactionType"": 1,
                                ""Capture"": true,
                                ""SaveCustomer"": false,
                                ""Customer"": {
                                    ""TokenCustomerID"": null,
                                    ""Reference"": """",
                                    ""Title"": ""Mr."",
                                    ""FirstName"": """",
                                    ""LastName"": """",
                                    ""CompanyName"": """",
                                    ""JobDescription"": """",
                                    ""Address"": {
                                        ""Street1"": """",
                                        ""Street2"": """",
                                        ""City"": """",
                                        ""State"": """",
                                        ""Country"": """",
                                        ""PostalCode"": """"
                                    },
                                    ""Phone"": """",
                                    ""Mobile"": """",
                                    ""Email"": """",
                                    ""Fax"": """",
                                    ""Url"": """",
                                    ""Comments"": """",
                                    ""IsActive"": false,
                                    ""RedirectURL"": null,
                                    ""CancelURL"": null,
                                    ""CardDetails"": {
                                        ""Name"": ""XXXXXXX"",
                                        ""Number"": ""444433XXXXXX1111"",
                                        ""ExpiryMonth"": ""01"",
                                        ""ExpiryYear"": ""23"",
                                        ""StartMonth"": null,
                                        ""StartYear"": null,
                                        ""IssueNumber"": null,
                                        ""CVN"": null
                                    },
                                    ""SecuredCardData"": null,
                                    ""CustomerIP"": null
                                },
                                ""ShippingDetails"": null,
                                ""PaymentDetails"": {
                                    ""TotalAmount"": 157850,
                                    ""InvoiceNumber"": """",
                                    ""InvoiceDescription"": """",
                                    ""InvoiceReference"": ""fdddc9a7-2555-4a8c-aa1d-6f2c200f4db5"",
                                    ""CurrencyCode"": ""AUD""
                                },
                                ""LineItems"": null,
                                ""Options"": null,
                                ""DeviceID"": null,
                                ""PartnerID"": null,
                                ""ThirdPartyWalletID"": null,
                                ""SecuredCardData"": null,
                                ""AuthTransactionID"": 0,
                                ""RedirectURL"": null,
                                ""CancelURL"": null,
                                ""CheckoutURL"": null,
                                ""CheckoutPayment"": false,
                                ""CustomerIP"": null,
                                ""CustomerReadOnly"": false,
                                ""Language"": null,
                                ""CustomView"": null,
                                ""VerifyCustomerPhone"": false,
                                ""VerifyCustomerEmail"": false,
                                ""HeaderText"": null,
                                ""LogoUrl"": null,
                                ""TransactionDateTime"": null,
                                ""FraudAction"": null,
                                ""TransactionCaptured"": null,
                                ""CurrencyCode"": null,
                                ""Source"": null,
                                ""MaxRefund"": null,
                                ""OriginalTransactionId"": null
                            },
                            ""TransactionStatus"": {
                                ""TransactionID"": 24461953,
                                ""Total"": 157850,
                                ""Status"": true,
                                ""Captured"": false,
                                ""BeagleScore"": -1.0,
                                ""FraudAction"": 0,
                                ""VerificationResult"": {
                                    ""CVN"": 0,
                                    ""Address"": 0,
                                    ""Email"": 0,
                                    ""Mobile"": 0,
                                    ""Phone"": 0,
                                    ""BeagleEmail"": 0,
                                    ""BeaglePhone"": 0
                                },
                                ""ProcessingDetails"": {
                                    ""AuthorisationCode"": ""786199"",
                                    ""ResponseCode"": ""00"",
                                    ""ResponseMessage"": ""A2000""
                                }
                            },
                            ""SharedPaymentUrl"": null,
                            ""FormActionUrl"": null,
                            ""AccessCode"": null,
                            ""AmexECEncryptedData"": null,
                            ""Errors"": null
                        }";
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var quoteAggregate = quote.Aggregate
                        .WithCustomerDetails(quote.Id)
                        .WithFormData(quote.Id)
                        .WithCalculationResult(quote.Id)
                        .WithCustomer()
                        .WithQuoteNumber(quote.Id);
            var paymentDetails = new Domain.Aggregates.Quote.PaymentDetails(PaymentGatewayName.EWay, 12345, "xxxxxx", JsonConvert.SerializeObject(transaction), response);
            var paymentMade = new PaymentMadeEvent(quote.Aggregate.TenantId, quote.Id, quote, paymentDetails, default, SystemClock.Instance.Now());
            return JsonConvert.SerializeObject(paymentMade);
        }

        private async Task<ReportBodyViewModel> GenerateReport(IEnumerable<ReportSourceDataType> sourceData, string paymentResponseJson, string gateway)
        {
            Guid tenantId = Guid.NewGuid();
            var tenant = TenantFactory.Create(tenantId);
            IEnumerable<Guid> productIds = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            var env = Domain.DeploymentEnvironment.Development;
            var fromDate = "2020-10-10".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            var toDate = "2020-10-20".ToLocalDateFromIso8601().GetInstantAtStartOfDayInZone(Timezones.AET);
            IEnumerable<TransactionType> transactionFilters =
                new TransactionType[]
                {
                    TransactionType.NewBusiness,
                    TransactionType.Renewal,
                    TransactionType.Adjustment,
                };

            var listPolicyTransactions = new List<IPolicyTransactionReadModelSummary>();
            listPolicyTransactions.Add(new FakePolicyTransactionReadModel("P0001", SystemClock.Instance.Now(), paymentResponseJson, gateway));

            var quoteItems = new List<IQuoteReportItem>();
            quoteItems.Add(new FakeQuoteReportItem("Q0001", SystemClock.Instance.Now(), paymentResponseJson, gateway));

            this.policyTransactionRepository.Setup(r => r.GetPolicyTransactions(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                productIds,
                env,
                fromDate,
                toDate,
                false,
                transactionFilters))
                .Returns(listPolicyTransactions);
            this.quoteReadModelRepository
                .Setup(r => r.GetQuoteDataForReports(tenant.Id, tenant.Details.DefaultOrganisationId, productIds, env, fromDate, toDate, false))
                .Returns(quoteItems);

            var dropCreationService = new DropGenerationService(
                this.policyTransactionRepository.Object,
                this.quoteReadModelRepository.Object,
                this.emailRepository.Object,
                this.configuration.Object,
                this.organisationService.Object,
                this.claimReadModelRepository.Object);
            return await dropCreationService.GenerateReportDrop(tenant.Id, tenant.Details.DefaultOrganisationId, productIds, env, sourceData, fromDate, toDate, false);
        }

        private class FakePolicyTransactionReadModel : IPolicyTransactionReadModelSummary
        {
            public FakePolicyTransactionReadModel(string policyNumber, Instant now)
            {
                this.PolicyNumber = policyNumber;
                this.CreatedTimestamp = now;
                this.AdjustmentEffectiveTimestamp = now;
                this.InceptionTimestamp = now;
                this.ExpiryTimestamp = now;
                this.EffectiveTimestamp = now;
                this.CancellationEffectiveTimestamp = now;
                this.CancellationEffectiveTicksSinceEpoch = now.ToUnixTimeTicks();
            }

            public FakePolicyTransactionReadModel(string policyNumber, Instant now, string paymentResponseJson, string gateway)
            {
                this.PolicyNumber = policyNumber;
                this.CreatedTimestamp = now;
                this.AdjustmentEffectiveTimestamp = now;
                this.InceptionTimestamp = now;
                this.ExpiryTimestamp = now;
                this.EffectiveTimestamp = now;
                this.CancellationEffectiveTimestamp = now;
                this.CancellationEffectiveTicksSinceEpoch = now.ToUnixTimeTicks();
                this.PaymentResponseJson = paymentResponseJson;
                this.PaymentGateway = gateway;
            }

            public string TransactionType => string.Empty;

            public string ProductName => string.Empty;

            public LocalDate CreatedDate => this.CreatedTimestamp.ToLocalDateInAet();

            public Instant CreatedTimestamp { get; }

            public LocalDateTime? AdjustmentEffectiveDateTime => this.AdjustmentEffectiveTimestamp?.ToLocalDateTimeInAet();

            public LocalDateTime? CancellationEffectiveDateTime => this.CancellationEffectiveTimestamp?.ToLocalDateTimeInAet();

            public Instant? AdjustmentEffectiveTimestamp { get; }

            public Instant? CancellationEffectiveTimestamp { get; }

            public string PolicyNumber { get; }

            public string QuoteReference => string.Empty;

            public string InvoiceNumber => string.Empty;

            public string CreditNoteNumber => string.Empty;

            public string CustomerFullName => string.Empty;

            public string CustomerEmail => string.Empty;

            public string RatingState => string.Empty;

            public PriceBreakdown PriceBreakdown => PriceBreakdown.Zero(PriceBreakdown.DefaultCurrencyCode);

            public bool IsTestData => false;

            public bool IsAdjusted => false;

            public long CancellationEffectiveTicksSinceEpoch { get; }

            public LocalDateTime? InceptionDateTime => this.InceptionTimestamp?.ToLocalDateTimeInAet();

            public Instant? InceptionTimestamp { get; }

            public LocalDateTime? ExpiryDateTime => this.ExpiryTimestamp?.ToLocalDateTimeInAet();

            public Instant? ExpiryTimestamp { get; }

            public LocalDateTime EffectiveDateTime => this.EffectiveTimestamp.ToLocalDateTimeInAet();

            public Instant EffectiveTimestamp { get; }

            public string FormData => "{\"formModel\": {\"data\": \"value\"}}";

            public string CalculationResult { get; set; } = null!;

            public string PaymentGateway { get; } = null!;

            public string PaymentResponseJson { get; } = null!;

            public CalculationResultReadModel LatestCalculationResult => throw new NotImplementedException();

            public string SerializedLatestCalculationResult { get; set; } = null!;

            public string OrganisationName { get; set; } = null!;

            public string OrganisationAlias { get; set; } = null!;

            public string AgentName { get; set; } = null!;
        }

        private class FakeQuoteReportItem : IQuoteReportItem
        {
            public FakeQuoteReportItem(string quoteNumber, Instant now)
            {
                this.QuoteNumber = quoteNumber;
                this.CreatedTimestamp = now;
                this.InceptionTimestamp = now;
                this.ExpiryTimestamp = now;
                this.EffectiveTimestamp = now;
                this.CancellationEffectiveTimestamp = now;
                this.CancellationEffectiveTicksSinceEpoch = now.ToUnixTimeTicks();
            }

            public FakeQuoteReportItem(string quoteNumber, Instant now, string paymentResponseJson, string gateway)
            {
                this.QuoteNumber = quoteNumber;
                this.CreatedTimestamp = now;
                this.InceptionTimestamp = now;
                this.ExpiryTimestamp = now;
                this.EffectiveTimestamp = now;
                this.CancellationEffectiveTimestamp = now;
                this.CancellationEffectiveTicksSinceEpoch = now.ToUnixTimeTicks();
                this.PaymentResponseJson = paymentResponseJson;
                this.PaymentGateway = gateway;
            }

            public string CustomerEmail => string.Empty;

            public string CustomerAlternativeEmail => string.Empty;

            public string CustomerMobilePhone => string.Empty;

            public string CustomerHomePhone => string.Empty;

            public string CustomerWorkPhone => string.Empty;

            public string CreditNoteNumber => string.Empty;

            public System.Guid QuoteId { get; set; }

            public string InvoiceNumber { get; set; } = null!;

            public string QuoteTitle { get; set; } = null!;

            public string QuoteNumber { get; set; } = null!;

            public NodaTime.Instant InvoiceTimestamp { get; set; }

            public NodaTime.Instant SubmissionTimestamp { get; set; }

            public NodaTime.Instant? PolicyInceptionTimestamp { get; set; }

            public bool IsInvoiced { get; set; }

            public bool IsPaidFor { get; set; }

            public string PolicyNumber { get; set; } = null!;

            public bool IsSubmitted { get; set; }

            public bool IsTestData { get; set; }

            public bool IsDiscarded { get; set; }

            public string ProductName { get; set; } = null!;

            public string NameOfProduct { get; set; } = null!;

            public string CustomerFullName { get; set; } = null!;

            public string CustomerPreferredName { get; set; } = null!;

            public NodaTime.Instant LastModifiedTimestamp { get; set; }

            public NodaTime.LocalDate PolicyExpiryDate { get; set; }

            public NodaTime.LocalDate PolicyInceptionDate { get; set; }

            public string QuoteState => string.Empty;

            public bool PolicyIssued { get; set; }

            public NodaTime.Instant CreatedTimestamp { get; set; }

            public NodaTime.Instant CancellationEffectiveTimestamp { get; set; }

            public NodaTime.Instant PolicyIssuedTimestamp { get; set; }

            public NodaTime.Instant? ExpiryTimestamp { get; set; }

            public bool ExpiryEnabled { get; set; }

            public QuoteType QuoteType { get; set; }

            public DeploymentEnvironment Environment { get; set; }

            public string LatestFormData => "{\"formModel\": {\"data\": \"value\"}}";

            public CalculationResultReadModel LatestCalculationResult => new CalculationResultReadModel(null!);

            public bool IsAdjusted { get; set; }

            public long CancellationEffectiveTicksSinceEpoch { get; set; }

            public NodaTime.LocalDate InceptionDate { get; set; }

            public NodaTime.LocalDate ExpiryDate { get; set; }

            public NodaTime.LocalDate CancellationEffectiveDate { get; set; }

            public NodaTime.LocalDate EffectiveDate { get; set; }

            public NodaTime.Instant InceptionTimestamp { get; set; }

            public NodaTime.Instant EffectiveTimestamp { get; set; }

            public NodaTime.Instant EffectiveEndTimestamp { get; set; }

            public System.Guid? PolicyId { get; set; }

            public System.Guid AggregateId { get; set; }

            public Guid TenantId { get; set; }

            public Guid OrganisationId { get; set; }

            public Guid ProductId { get; set; }

            public System.Guid? OwnerUserId { get; set; }

            public System.Guid? CustomerId { get; set; }

            public string PaymentGateway { get; } = null!;

            public string PaymentResponseJson { get; } = null!;

            public string ProductAlias => throw new NotImplementedException();

            public LocalDate PolicyEffectiveDate => throw new NotImplementedException();

            public string OrganisationName { get; } = null!;

            public string OrganisationAlias { get; } = null!;

            public string AgentName { get; } = null!;

            public Instant PolicyEffectiveTimestamp => throw new NotImplementedException();

            public Instant PolicyEffectiveEndTimestamp => throw new NotImplementedException();

            public Instant? PolicyExpiryTimestamp => throw new NotImplementedException();

            public DateTimeZone TimeZone => throw new NotImplementedException();

            public string TimeZoneId => throw new NotImplementedException();

            string IBaseReportReadModel.SerializedLatestCalculationResult { get; } = null!;

            public Instant? PolicyTransactionEffectiveTimestamp { get; }

            public Guid? ProductReleaseId { get; set; }

            public int? ProductReleaseMajorNumber { get; set; }

            public int? ProductReleaseMinorNumber { get; set; }
        }

        private class FakeEmailReportItem : IEmailDetails
        {
            public FakeEmailReportItem(string subject, Instant now)
            {
                this.Subject = subject;
                this.CreatedTimestamp = now;
            }

            public System.Guid Id { get; set; }

            public Guid OrganisationId { get; set; }

            public Guid TenantId { get; set; }

            public Guid? ProductId { get; set; }

            public string ProductName { get; set; } = null!;

            public DeploymentEnvironment Environment { get; set; }

            public string Recipient => "garry.recipient@ubind.io";

            public string From => "garry.from@ubind.io";

            public string CC => string.Empty;

            public string BCC => string.Empty;

            public string Subject { get; set; }

            public string HtmlMessage => string.Empty;

            public string PlainMessage => string.Empty;

            public CustomerData Customer { get; set; } = null!;

            public PolicyData Policy { get; set; } = null!;

            public QuoteData Quote { get; set; } = null!;

            public ClaimData Claim { get; set; } = null!;

            public PolicyTransactionData PolicyTransaction { get; set; } = null!;

            public UserData User { get; set; } = null!;

            OrganisationData IEmailDetails.Organisation { get; } = null!;

            public IEnumerable<EmailAttachment> EmailAttachments => Array.Empty<EmailAttachment>();

            public IEnumerable<DocumentFile> Documents => Array.Empty<DocumentFile>();

            public EmailType EmailType { get; set; }

            public Instant CreatedTimestamp { get; set; }

            public IEnumerable<Tag> Tags { get; set; } = null!;

            public IEnumerable<Relationship> Relationships { get; set; } = null!;

            public IEnumerable<FileContent> FileContents { get; set; } = null!;

            public void AddEmailAttachments(ICollection<EmailAttachment> emailAttachments)
            {
                throw new System.NotImplementedException();
            }
        }

        private class FakeClaimReportItem : IClaimReportItem
        {
            public FakeClaimReportItem()
            {
                // this.TenantId = Guid.NewGuid();
                // this.ProductId = Guid.NewGuid();
                // this.OrganisationId = Guid.NewGuid();
                this.OrganisationName = "fake organisation";
                this.OrganisationAlias = "fake-organisation";
                this.AgentName = "fake agent";
                this.ProductName = "fake product name";
                this.CustomerFullName = "Fake Customer Name";
                this.ClaimNumber = "FAKE0001";
                this.Amount = Random.Shared.Next(100000, 200000);
                this.CreatedTimestamp = SystemClock.Instance.Now();
                this.LastModifiedTimestamp = SystemClock.Instance.Now();
                this.Status = "complete";
                this.WorkflowStep = "complete";
                this.Documents = new List<ClaimAttachmentReadModel>();
            }

            public FakeClaimReportItem(
                    Guid organisationId,
                    string organisationName,
                    string organisationAlias,
                    string productName,
                    string agentName,
                    string customerName,
                    string claimNumber,
                    decimal amount,
                    Instant createDate,
                    Instant modifyDate)
            {
                this.TenantId = Guid.NewGuid();
                this.ProductId = Guid.NewGuid();
                this.OrganisationId = organisationId;
                this.OrganisationName = organisationName;
                this.OrganisationAlias = organisationAlias;
                this.AgentName = agentName;
                this.ProductName = productName;
                this.CustomerFullName = customerName;
                this.ClaimNumber = claimNumber;
                this.Amount = amount;
                this.CreatedTimestamp = createDate;
                this.LastModifiedTimestamp = modifyDate;
                this.Status = "complete";
                this.WorkflowStep = "complete";
                this.Documents = new List<ClaimAttachmentReadModel>();
            }

            public string CustomerEmail { get; set; } = string.Empty;

            public string CustomerAlternativeEmail { get; set; } = string.Empty;

            public string CustomerMobilePhone { get; set; } = string.Empty;

            public string CustomerHomePhone { get; set; } = string.Empty;

            public string CustomerWorkPhone { get; set; } = string.Empty;

            public string CreditNoteNumber { get; set; } = string.Empty;

            public string OrganisationName { get; set; } = string.Empty;

            public string OrganisationAlias { get; set; } = string.Empty;

            public string AgentName { get; set; } = string.Empty;

            public string? InvoiceNumber { get; set; }

            public string ProductName { get; set; } = string.Empty;

            public Guid OrganisationId { get; set; }

            public Guid ProductId { get; set; }

            public DeploymentEnvironment Environment { get; set; }

            public Guid? PolicyId { get; set; }

            public string PolicyNumber { get; set; } = string.Empty;

            public Guid? CustomerId { get; set; }

            public Guid? PersonId { get; set; }

            public string CustomerFullName { get; set; } = string.Empty;

            public Guid? OwnerUserId { get; set; }

            public string CustomerPreferredName { get; set; } = string.Empty;

            public Guid? CustomerOwnerUserId { get; set; }

            public string ClaimReference { get; set; } = string.Empty;

            public string ClaimNumber { get; set; } = string.Empty;

            public decimal? Amount { get; set; }

            public string Description { get; set; } = string.Empty;

            public LocalDateTime? IncidentDateTime { get; set; }

            public Instant? IncidentTimestamp { get; set; }

            public bool IsTestData { get; set; }

            public string Status { get; set; } = string.Empty;

            public Guid LatestCalculationResultId { get; set; }

            public string WorkflowStep { get; set; } = string.Empty;

            public IClaimCalculationResultReadModel? LatestCalculationResult { get; set; }

            public Guid LatestCalculationResultFormDataId { get; set; }

            public string LatestFormData { get; set; } = string.Empty;

            public DateTimeZone? TimeZone { get; set; }

            public IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }

            public Instant CreatedTimestamp { get; set; }

            public Instant LastModifiedTimestamp { get; set; }

            public Guid Id { get; set; } = Guid.Empty;

            public Guid TenantId { get; set; } = Guid.Empty;

            public string PaymentGateway { get; set; } = string.Empty;

            public string PaymentResponseJson { get; set; } = null!;

            public string SerializedLatestCalculationResult { get; set; } = null!;

            public string GetFormData()
            {
                return "{}";
            }
        }
    }
}
