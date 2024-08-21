// <copyright file="PolicyDocumentController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for handling portal-related quote requests.
    /// </summary>
    [MustBeLoggedIn]
    [Route("/api/v1/{environment}/policy/{policyId}/transaction/{transactionId}/document")]
    public class PolicyDocumentController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IPolicyService policyService;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDocumentController"/> class.
        /// </summary>
        /// <param name="policyService">The policy service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public PolicyDocumentController(
            IPolicyService policyService,
            IAuthorisationService authorisationService,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.policyService = policyService;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Gets the details of a quote.
        /// </summary>
        /// <param name="policyId">The ID of the policy the document belongs to.</param>
        /// <param name="transactionId">THe ID of the transaction which the document is associated with.</param>
        /// <param name="documentId">The ID of the document to retrieve.</param>
        /// <param name="environment">Deployment Environment.</param>
        /// <returns>The document.</returns>
        [HttpGet]
        [Route("{documentId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPolicyTransactionDocument(Guid policyId, Guid transactionId, Guid documentId, DeploymentEnvironment environment)
        {
            IPolicyReadModelDetails policy = await this.policyService.GetPolicy(this.User.GetTenantId(), policyId);
            await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policy);
            var tenant = await this.cachingResolver.GetTenantOrThrow(this.User.GetTenantId());
            var isMutual = TenantHelper.IsMutual(tenant.Details.Alias);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatch(
                policy.Environment,
                environment,
                TenantHelper.CheckAndChangeTextToMutual("policy", isMutual));

            var transaction = policy.Transactions.SingleOrDefault(t => t.PolicyTransaction.Id == transactionId);
            if (transaction == null)
            {
                return Errors.General.NotFound(TenantHelper.CheckAndChangeTextToMutual("policy transaction", isMutual), transactionId).ToProblemJsonResult();
            }
            var fileContent = this.policyService.GetPolicyDocumentContent(tenant.Id, documentId);
            if (fileContent == null)
            {
                return Errors.General.NotFound("document", documentId).ToProblemJsonResult();
            }

            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }
    }
}
