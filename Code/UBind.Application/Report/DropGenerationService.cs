// <copyright file="DropGenerationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Report
{
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Persistence.ReadModels.Claim;

    /// <summary>
    /// Service for generating report models.
    /// </summary>
    public class DropGenerationService : IDropGenerationService
    {
        private readonly IPolicyTransactionReadModelRepository policyTransactionRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IEmailRepository emailRepository;
        private readonly IEmailInvitationConfiguration emailInvitationConfiguration;
        private readonly IOrganisationService organisationService;
        private readonly IClaimReadModelRepository claimReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropGenerationService"/> class.
        /// </summary>
        /// <param name="policyTransactionRepository">The policy transaction repository.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        /// <param name="emailRepository">The email repository.</param>
        /// <param name="emailInvitationConfiguration">The configuration for email invitations.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="claimReadModelRepository">The Claim Read Repository</param>
        public DropGenerationService(
            IPolicyTransactionReadModelRepository policyTransactionRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IEmailRepository emailRepository,
            IEmailInvitationConfiguration emailInvitationConfiguration,
            IOrganisationService organisationService,
            IClaimReadModelRepository claimReportRepository)
        {
            this.policyTransactionRepository = policyTransactionRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.emailRepository = emailRepository;
            this.emailInvitationConfiguration = emailInvitationConfiguration;
            this.organisationService = organisationService;
            this.claimReadModelRepository = claimReportRepository;
        }

        /// <inheritdoc/>
        public async Task<ReportBodyViewModel> GenerateReportDrop(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            IEnumerable<ReportSourceDataType> sourceData,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData)
        {
            var policyTransactionFilter = this.GeneratePolicyTransactionFilterFromReportSourceData(sourceData);

            IEnumerable<PolicyTransactionViewModel> policyTransactions = new List<PolicyTransactionViewModel>();

            var fromDefaultOrganisation = await this.organisationService.IsOrganisationDefaultForTenant(
                tenantId, organisationId);
            var reportOrganisationId = fromDefaultOrganisation ? default : organisationId;

            if (policyTransactionFilter.Any())
            {
                var policyTransactionQuery = this.policyTransactionRepository.GetPolicyTransactions(
                    tenantId,
                    reportOrganisationId,
                    productIds.ToArray(),
                    environment,
                    fromTimestamp,
                    toTimestamp,
                    includeTestData,
                    policyTransactionFilter);

                policyTransactions = policyTransactionQuery.ToList().Select(p => new PolicyTransactionViewModel(p));
            }

            IEnumerable<QuoteViewModel> quotes = new List<QuoteViewModel>();

            if (sourceData.Contains(ReportSourceDataType.Quote))
            {
                IEnumerable<IQuoteReportItem> quoteReadModelSummaryQueries
                    = this.quoteReadModelRepository.GetQuoteDataForReports(
                        tenantId, reportOrganisationId, productIds, environment, fromTimestamp, toTimestamp, includeTestData);
                quotes = quoteReadModelSummaryQueries.ToList().Select(p => new QuoteViewModel(p));
            }

            List<EmailViewModel> emails = new List<EmailViewModel>();

            if (sourceData.Contains(ReportSourceDataType.SystemEmail))
            {
                var systemEmail = this.emailRepository.GetSystemEmailForReport(tenantId, reportOrganisationId, environment, fromTimestamp, toTimestamp);
                emails.AddRange(systemEmail.Select(e => new EmailViewModel(e, this.emailInvitationConfiguration.InvitationLinkHost)));
            }

            if (sourceData.Contains(ReportSourceDataType.ProductEmail))
            {
                var productEmail = this.emailRepository.GetProductEmailForReport(
                    tenantId, reportOrganisationId, productIds, environment, fromTimestamp, toTimestamp, includeTestData);
                emails.AddRange(productEmail.Select(e => new EmailViewModel(e, this.emailInvitationConfiguration.InvitationLinkHost)));
            }

            IEnumerable<ClaimViewModel> claims = new List<ClaimViewModel>();
            if (sourceData.Contains(ReportSourceDataType.Claim))
            {
                IEnumerable<IClaimReportItem> claimReadModelSummaryQueries = this.claimReadModelRepository.GetClaimsDataForReports(
                        tenantId, reportOrganisationId, productIds, environment, fromTimestamp, toTimestamp, includeTestData);
                claims = claimReadModelSummaryQueries.ToList().Select(p => new ClaimViewModel(p));
            }

            return new ReportBodyViewModel(policyTransactions, quotes, emails, claims);
        }

        private IEnumerable<TransactionType> GeneratePolicyTransactionFilterFromReportSourceData(
           IEnumerable<ReportSourceDataType> reportSourceDataTypes)
        {
            foreach (var reportSourceDataType in reportSourceDataTypes)
            {
                if (reportSourceDataType == ReportSourceDataType.NewBusiness)
                {
                    yield return TransactionType.NewBusiness;
                }
                else if (reportSourceDataType == ReportSourceDataType.Renewal)
                {
                    yield return TransactionType.Renewal;
                }
                else if (reportSourceDataType == ReportSourceDataType.Adjustment)
                {
                    yield return TransactionType.Adjustment;
                }
                else if (reportSourceDataType == ReportSourceDataType.Cancellation)
                {
                    yield return TransactionType.Cancellation;
                }
            }
        }
    }
}
