// <copyright file="ClaimWorkflowOperationsController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Quoter
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Queries.Claim;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Web.Filters;
    using UBind.Web.Helpers;
    using UBind.Web.ResourceModels.Claim;

    /// <summary>
    /// Controller for endorsement-related requests.
    /// </summary>
    [Produces("application/json")]
    [Route("/api/v1/{tenant}/{environment}/{product}/claim")]
    [RequiresFeature(Feature.ClaimsManagement)]
    public class ClaimWorkflowOperationsController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IClaimAggregateRepository claimAggregateRepository;
        private readonly ICqrsMediator mediator;
        private readonly ICustomerHelper customerHelper;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimWorkflowOperationsController"/> class.
        /// </summary>
        /// <param name="claimAggregateRepository">The claim aggregate repository.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="customerHelper">The customer helper.</param>
        /// <param name="authorisationService">The authorization service.</param>
        public ClaimWorkflowOperationsController(
            ICachingResolver cachingResolver,
            IClaimAggregateRepository claimAggregateRepository,
            ICqrsMediator mediator,
            ICustomerHelper customerHelper,
            IAuthorisationService authorisationService)
        {
            this.cachingResolver = cachingResolver;
            this.claimAggregateRepository = claimAggregateRepository;
            this.mediator = mediator;
            this.customerHelper = customerHelper;
            this.authorisationService = authorisationService;
        }

        /// <summary>
        /// Moves a claim to incomplete state.
        /// </summary>
        /// <param name="model">The claim model.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("actualise")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Actualise(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Actualise);
        }

        /// <summary>
        /// Approve a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("autoApproval")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AutoApproveClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.AutoApproval);
        }

        /// <summary>
        /// Notify a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("notify")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> NotifyClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Notify);
        }

        /// <summary>
        /// Acknowledge a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.AcknowledgeClaimNotifications)]
        [Route("acknowledge")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AcknowledgeClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Acknowledge);
        }

        /// <summary>
        /// Acknowledge a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("return")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReturnClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Return);
        }

        /// <summary>
        /// Review claim Referral.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("reviewReferral")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReviewReferralClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.ReviewReferral);
        }

        /// <summary>
        /// Review claim approval.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.ReviewClaims)]
        [Route("reviewApproval")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReviewApprovalClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.ReviewApproval);
        }

        /// <summary>
        /// Assessment of claim referral.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("assessmentReferral")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssessmentReferralClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.AssessmentReferral);
        }

        /// <summary>
        /// Assessment of claim approval.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHavePermission(Permission.AssessClaims)]
        [Route("assessmentApproval")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssessmentApprovalClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.AssessmentApproval);
        }

        /// <summary>
        /// Decline a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("decline")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeclineClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Decline);
        }

        /// <summary>
        /// Withdraw a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("withdraw")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Withdraw);
        }

        /// <summary>
        /// Settle a claim.
        /// </summary>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("settle")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SettleClaim(string tenant, [FromBody] ClaimFormDataUpdateModel model)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, model.ClaimId);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            return await this.ChangeClaimState(tenantModel.Id, model, ClaimActions.Settle);
        }

        /// <summary>
        /// Creates or updates a customer for a claim.
        /// </summary>
        /// <param name="tenant">The Id or Alias of the tenant to quote on.</param>
        /// <param name="product">The ID of the product to quote on.</param>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">Resource model with new form data.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [ValidateModel]
        [Route("customer")]
        [ProducesResponseType(typeof(ClaimApplicationModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Customer(
            string tenant, string product, DeploymentEnvironment environment, [FromBody] CustomerUpdateModel model)
        {
            // TODO: Check operation does not send additional data, and if so, what is meaning of updates accompanying form?
            await this.authorisationService.ThrowIfUserIsAuthenticatedAndCannotModifyClaim(this.User, model.ClaimId);
            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            var claimAggregate = this.claimAggregateRepository.GetById(productModel.TenantId, model.ClaimId);
            WebFormValidator.ValidateClaimRequest(model.ClaimId, claimAggregate, new ProductContext(productModel.TenantId, productModel.Id, environment));
            model.CustomerDetails.FixNameComponents();
            var resolvedCustomerDetails = await this.customerHelper.CreateResolvedCustomerPersonalDetailsModel(
                    productModel.TenantId, model?.CustomerDetails, claimAggregate.OrganisationId);
            if (!claimAggregate.HasCustomer)
            {
                await this.mediator.Send(new CreateCustomerForClaimCommand(
                    productModel.TenantId,
                    environment,
                    claimAggregate,
                    resolvedCustomerDetails,
                    claimAggregate.OwnerUserId,
                    model.PortalId,
                    claimAggregate.IsTestData));
            }
            else
            {
                await this.mediator.Send(new UpdateCustomerCommand(
                    productModel.TenantId,
                    claimAggregate.CustomerId.Value,
                    resolvedCustomerDetails));
            }

            var readModel = await this.mediator.Send(new GetClaimByIdQuery(productModel.TenantId, model.ClaimId));
            var outputModel = new ClaimApplicationModel(readModel);
            return this.Ok(outputModel);
        }

        private async Task<IActionResult> ChangeClaimState(Guid tenantId, ClaimFormDataUpdateModel model, ClaimActions operation)
        {
            string formDataJson = model.FormDataJson == null ? null : JsonConvert.SerializeObject(model.FormDataJson);
            var readModel = await this.mediator.Send(new ChangeClaimStateCommand(tenantId, model.ClaimId, operation, formDataJson));
            return this.Ok(new ClaimApplicationModel(readModel));
        }
    }
}
