// <copyright file="ClaimController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Claim;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Queries.Claim;
    using UBind.Application.Queries.Claim.Version;
    using UBind.Application.Queries.ProductRelease;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Validation;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Claim;

    /// <summary>
    /// Controller for handling claims-related requests.
    /// </summary>
    [Route("api/v1/claim")]
    [Produces("application/json")]
    public class ClaimController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IClaimService claimService;
        private readonly Application.User.IUserService userService;
        private readonly IConfigurationService configurationService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IFormDataPrettifier formDataPrettifier;
        private readonly IPolicyService policyService;
        private readonly IOrganisationService organisationService;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICustomerService customerService;
        private readonly IFeatureSettingService settingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimController"/> class.
        /// </summary>
        /// <param name="claimService">The claims service.</param>
        /// <param name="userService">The user service of <see cref="Application.User"/>.</param>
        /// <param name="configurationService">The configuration retrieval service.</param>
        /// <param name="productConfigurationProvider">The product configuration provider.</param>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="policyService">Service for creating policies..</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        /// <param name="settingService">The feature setting service.</param>
        public ClaimController(
            IClaimService claimService,
            Application.User.IUserService userService,
            IConfigurationService configurationService,
            IProductConfigurationProvider productConfigurationProvider,
            IFormDataPrettifier formDataPrettifier,
            IPolicyService policyService,
            IOrganisationService organisationService,
            IClaimReadModelRepository claimReadModelRepository,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            ICustomerService customerService,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IFeatureSettingService settingService)
        {
            this.cachingResolver = cachingResolver;
            this.claimService = claimService;
            this.userService = userService;
            this.configurationService = configurationService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.formDataPrettifier = formDataPrettifier;
            this.policyService = policyService;
            this.organisationService = organisationService;
            this.claimReadModelRepository = claimReadModelRepository;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.customerService = customerService;
            this.settingService = settingService;
        }

        /// <summary>
        /// Gets the list of claims in the system with the status provided.
        /// </summary>
        /// <param name="options">The filter options to be used.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [RequiresFeature(Feature.ClaimsManagement)]
        [ProducesResponseType(typeof(IEnumerable<ClaimSetModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaims([FromQuery] QueryOptionsModel options)
        {
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options, true);
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(options.Tenant));
            EntityListFilters filters = await options.ToFilters(tenantModel.Id, this.cachingResolver, nameof(ClaimReadModel.CreatedTicksSinceEpoch));
            await this.authorisationService.ApplyViewClaimRestrictionsToFilters(this.User, filters);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);

            var query = new GetClaimsMatchingFiltersQuery(tenantModel.Id, filters);
            var claims = await this.mediator.Send(query);

            var models = claims.Select(claim => new ClaimSetModel(claim));
            return this.Ok(models);
        }

        /// <summary>
        /// Gets the list of claims in the system with the status provided.
        /// </summary>
        /// <param name="options">The filter options to be used.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [Route("periodic-summary")]
        [MustHaveUserType(UserType.Client)]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [RequiresFeature(Feature.ClaimsManagement)]
        [ProducesResponseType(typeof(IEnumerable<ClaimSetModel>), StatusCodes.Status200OK)]
        [SingleValueQueryParameter(
            nameof(BasePeriodicSummaryQueryOptionsModel.Environment),
            nameof(BasePeriodicSummaryQueryOptionsModel.SamplePeriodLength),
            nameof(BasePeriodicSummaryQueryOptionsModel.FromDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.ToDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.CustomSamplePeriodMinutes),
            nameof(BasePeriodicSummaryQueryOptionsModel.Timezone))]
        public async Task<IActionResult> GetPeriodicSummary([FromQuery] ClaimPeriodicSummaryQueryOptionsModel options)
        {
            var tenantId = this.User.GetTenantId();
            options.ValidateQueryOptions();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options, true);
            EntityListFilters filters = await options.ToFilters(tenantId, this.cachingResolver);
            await this.authorisationService.ApplyViewClaimRestrictionsToFilters(this.User, filters);
            await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);
            var claimSummaries = await this.mediator.Send(new GetClaimPeriodicSummariesQuery(tenantId, filters, options));
            if (options.SamplePeriodLength == SamplePeriodLength.All && claimSummaries.Any())
            {
                return this.Ok(claimSummaries.First());
            }

            return this.Ok(claimSummaries);
        }

        /// <summary>
        /// Retrieves the claim record with the given ID.
        /// </summary>
        /// <param name="claimId">The ID of the claim to be retrieved.</param>
        /// <param name="environment">The environment where the request was sent from.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [Route("{claimId}")]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [RequiresFeature(Feature.ClaimsManagement)]
        [ProducesResponseType(typeof(ClaimDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaimDetails([NotEmpty] Guid claimId, [FromQuery] string environment)
        {
            var claim = await this.mediator.Send(new GetClaimDetailsByIdQuery(this.User.GetTenantId(), claimId));
            await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claim);
            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
            var isMutual = TenantHelper.IsMutual(tenantAlias);
            EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(
                claim.Environment,
                environment,
                TenantHelper.CheckAndChangeTextToMutual("claim", isMutual));

            // TODO: Instead of pulling out these entities separately, create a ClaimReadModelDetail object
            // which is created using database joins.
            IUserReadModelSummary owner = null;
            if (claim.OwnerUserId.HasValue)
            {
                owner = this.userService.GetUser(claim.TenantId, claim.OwnerUserId.Value);
            }

            var customer = claim.CustomerId.HasValue ? this.customerService.GetCustomerById(claim.TenantId, claim.CustomerId.Value) : default;
            var releaseContext = await this.mediator.Send(new GetDefaultProductReleaseContextOrThrowQuery(
                claim.TenantId,
                claim.ProductId,
                claim.Environment));
            var displayableFields = await this.configurationService.GetDisplayableFieldsAsync(
                releaseContext,
                WebFormAppType.Claim);
            var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
                releaseContext,
                WebFormAppType.Claim);
            var questionAttachmentKeys = await this.GetQuestionAttachments(releaseContext);
            var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                claim.TenantId,
                Domain.Enums.AdditionalPropertyEntityType.Claim,
                claimId);
            var claimModel = new ClaimDetailModel(
                this.formDataPrettifier, claim, formDataSchema, additionalPropertyValueDtos, owner, customer, displayableFields);
            claimModel.QuestionAttachmentKeys = questionAttachmentKeys;
            return this.Ok(claimModel);
        }

        /// <summary>
        /// Creates a new claim.
        /// </summary>
        /// <param name="claimCreateModel">The data model for creating a claim.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ValidateModel]
        [RequiresFeature(Feature.ClaimsManagement)]
        [ProducesResponseType(typeof(ClaimSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateClaim([FromBody] ClaimCreateModel claimCreateModel)
        {
            Guid claimId = default;
            string tenantId = claimCreateModel.Tenant ?? this.User.GetTenantId().ToString();

            var tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(tenantId));
            if (tenant == null)
            {
                throw new ErrorException(Errors.General.ModelValidationFailed(
                    "The \"tenant\" property was not set, and when you are not logged in, it's required."));
            }

            var ownerIdRequest = new DetermineOwnerIdForNewClaimQuery(
                tenant.Id,
                this.User,
                claimCreateModel.CustomerId);
            var ownerUserId = await this.mediator.Send(ownerIdRequest);

            if (!claimCreateModel.PolicyId.HasValue)
            {
                // we'll need the product context
                if (claimCreateModel.Product == null)
                {
                    throw new ErrorException(Errors.General.ModelValidationFailed(
                        "The \"product\" property was not set, and when no policyId is passed, it's required."));
                }

                if (claimCreateModel.Environment == null)
                {
                    throw new ErrorException(Errors.General.ModelValidationFailed(
                        "The \"environment\" property was not set, and when no policyId is passed, it's required."));
                }

                Guid organisationId = tenant.Details.DefaultOrganisationId;
                if (claimCreateModel.Organisation != null)
                {
                    if (!Guid.TryParse(claimCreateModel.Organisation, out organisationId))
                    {
                        organisationId = this.organisationService.GetOrganisationSummaryForTenantAliasAndOrganisationAlias(
                            tenant.Details.Alias, claimCreateModel.Organisation).Id;
                    }
                }

                Product product = await this.cachingResolver.GetProductOrThrow(tenant.Id, new GuidOrAlias(claimCreateModel.Product));
                claimId = await this.mediator.Send(new CreateClaimCommand(
                    tenant.Id,
                    organisationId,
                    product.Id,
                    claimCreateModel.Environment.Value,
                    claimCreateModel.IsTestData,
                    claimCreateModel.CustomerId,
                    ownerUserId,
                    Timezones.AET));
            }
            else
            {
                // check the user has access to this policy
                IPolicyReadModelSummary policy =
                    await this.policyService.GetPolicy(this.User.GetTenantId(), claimCreateModel.PolicyId.Value);
                await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policy);

                claimId = await this.mediator.Send(new CreateClaimForPolicyCommand(
                    tenant.Id, claimCreateModel.PolicyId.Value, ownerUserId));
            }

            var claim = this.claimReadModelRepository.GetSummaryById(tenant.Id, claimId);
            return this.Ok(new ClaimSetModel(claim));
        }

        /// <summary>
        /// Updates an existing entry for claims record.
        /// </summary>
        /// <param name="claimId">The ID of the claim that will have its claim number unassigned.</param>
        /// <param name="environment">The current environment.</param>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustBeLoggedIn]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("{claimId}/assign-claim-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignClaimNumber(
            Guid claimId,
            [FromQuery] string environment,
            [FromBody] ClaimNumberAssignmentModel model)
        {
            var claimReadModel = await this.mediator.Send(new GetClaimSummaryByIdQuery(this.User.GetTenantId(), claimId));
            await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claimReadModel);
            var successEnvironment = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!successEnvironment)
            {
                return await Task.FromResult<IActionResult>(
                    this.NotFound($"Environment '{environment}' cannot be found"));
            }

            try
            {
                await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, claimId);
                await this.authorisationService.ThrowIfUserCannotUpdateClaimNumber(this.User);
                await this.claimService.AssignClaimNumber(
                    this.User.GetTenantId(), claimId, model.ClaimNumber, env, model.IsRestoreToList);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already assigned"))
                {
                    return this.Conflict(ex.Message);
                }
                else
                {
                    return this.BadRequest(ex.Message);
                }
            }

            return this.Ok();
        }

        /// <summary>
        /// Unassign a claim number from a claim.
        /// </summary>
        /// <param name="claimId">The ID of the claim that will have its claim number unassigned.</param>
        /// <param name="environment">The current environment.</param>
        /// <param name="model">The updated claim.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustBeLoggedIn]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("{claimId}/unassign-claim-number")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnassignClaimNumber(
            Guid claimId, string environment, [FromBody] ClaimNumberAssignmentModel model)
        {
            var successEnvironment = Enum.TryParse(environment, true, out DeploymentEnvironment env);
            if (!successEnvironment)
            {
                return await Task.FromResult<IActionResult>(
                    this.NotFound($"Environment '{environment}' cannot be found"));
            }

            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, claimId);
            await this.authorisationService.ThrowIfUserCannotUpdateClaimNumber(this.User);
            await this.claimService.UnassignClaimNumber(
                this.User.GetTenantId(), claimId, env, model.IsRestoreToList);
            return this.Ok();
        }

        /// <summary>
        /// Withdraw a claim.
        /// </summary>
        /// <param name="claimId">The ID of the claim that will be withdrawn.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustBeLoggedIn]
        [ValidateModel]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("{claimId}/withdraw")]
        [ProducesResponseType(typeof(ClaimSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> WithdrawClaim(Guid claimId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, claimId);
            var claimReadModel = await this.mediator.Send(new GetClaimSummaryByIdQuery(this.User.GetTenantId(), claimId));
            var readModel = await this.mediator.Send(new ChangeClaimStateCommand(claimReadModel.TenantId, claimId, ClaimActions.Withdraw));
            return this.Ok(new ClaimSetModel(claimReadModel));
        }

        /// <summary>
        /// Gets the list of version records of a claim.
        /// </summary>
        /// <param name="claimId">The ID of the parent claim for the version.</param>
        /// <param name="environment">The environment where the request originated from.</param>
        /// <returns>Ok.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [Route("{claimId}/version")]
        [ProducesResponseType(typeof(IEnumerable<ClaimVersionListModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaimVersions(Guid claimId, [FromQuery] string environment)
        {
            await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claimId);
            var claimVersionsList = await this.mediator.Send(
                new GetClaimVersionDetailsByClaimIdQuery(this.User.GetTenantId(), claimId));
            var models = claimVersionsList.Select(x => new ClaimVersionListModel(x));
            return this.Ok(models);
        }

        /// <summary>
        /// Associate claim with policy.
        /// </summary>
        /// <param name="claimId">The claim ID.</param>
        /// <param name="policyId">The Policy ID.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ManageClaims, Permission.ManageAllClaims, Permission.ManageAllClaimsForAllOrganisations)]
        [Route("{claimId}/associate-with-policy/{policyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> AssociateClaimWithPolicy(Guid claimId, Guid policyId)
        {
            await this.authorisationService.ThrowIfUserCannotAssociateClaimWithPolicy(this.User);
            await this.authorisationService.ThrowIfUserCannotModifyClaim(this.User, claimId);
            var claimReadModel = await this.mediator.Send(new GetClaimSummaryByIdQuery(this.User.GetTenantId(), claimId));
            await this.claimService.AssociateClaimWithPolicyAsync(claimReadModel.TenantId, policyId, claimId);
            return this.Ok($"Claim Id {claimId} has been associated with Policy Id {policyId}.");
        }

        /// <summary>
        /// Retrieves the claim version record with the given ID.
        /// </summary>
        /// <param name="claimVersionId">The ID of the version of the claim to be retrieved.</param>
        /// <param name="environment">The environment where the request was sent from.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [Route("{claimId}/version/{claimVersionId}")]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [ProducesResponseType(typeof(ClaimVersionDetailModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaimVersionDetails(Guid claimVersionId, [FromQuery] string environment)
        {
            var userTenantId = this.User.GetTenantId();
            var claimVersion = await this.mediator.Send(new GetClaimVersionDetailsByIdQuery(userTenantId, claimVersionId));
            if (claimVersion == null)
            {
                return this.NotFound();
            }

            var claim = await this.mediator.Send(new GetClaimSummaryByIdQuery(this.User.GetTenantId(), claimVersion.ClaimId));
            await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claim);

            var policy = claim.PolicyId.HasValue ? await this.policyService.GetPolicy(this.User.GetTenantId(), claim.PolicyId.Value) : null;
            var policyOwner = policy != null && policy.OwnerUserId != null ? this.userService.GetUser(policy.TenantId, policy.OwnerUserId.Value) : null;
            var customer = claim.CustomerId.HasValue ? this.customerService.GetCustomerById(claim.TenantId, claim.CustomerId.Value) : null;
            var releaseContext = await this.mediator.Send(new GetDefaultProductReleaseContextOrThrowQuery(
                claim.TenantId,
                claim.ProductId,
                claim.Environment));
            var displayableFields = await this.configurationService.GetDisplayableFieldsAsync(
                releaseContext, WebFormAppType.Claim);
            var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
                releaseContext, WebFormAppType.Claim);
            var questionAttachments = await this.GetQuestionAttachments(releaseContext);
            var additionalPropertyValueDtos = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                claim.TenantId,
                Domain.Enums.AdditionalPropertyEntityType.ClaimVersion,
                claimVersionId);
            var claimVersionModel = new ClaimVersionDetailModel(
                this.formDataPrettifier,
                claim,
                claimVersion,
                policy,
                formDataSchema,
                additionalPropertyValueDtos,
                policyOwner,
                customer,
                displayableFields);
            claimVersionModel.QuestionAttachmentKeys = questionAttachments;

            return this.Ok(claimVersionModel);
        }

        /// <summary>
        /// Gets the claim document by Id.
        /// </summary>
        /// <param name="documentId">THe ID of the document to retrieve.</param>
        /// <param name="claimId">The ID of the claim the document is for.</param>
        /// <returns>The document.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [Route("/api/v1/{environment}/claim/{claimId}/documents/{documentId}")]
        [MustHaveOneOfPermissions(Permission.ViewClaims, Permission.ViewAllClaims, Permission.ViewAllClaimsFromAllOrganisations)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaimDocument(Guid documentId, Guid claimId)
        {
            await this.authorisationService.ThrowIfUserCannotViewClaim(this.User, claimId);
            var fileContent = this.claimService
                .GetClaimDocumentContent(this.User.GetTenantId(), documentId, claimId);
            var fileContentResult = new FileContentResult(fileContent.FileContent, fileContent.ContentType);
            return fileContentResult;
        }

        private async Task<string[]> GetQuestionAttachments(ReleaseContext releaseContext)
        {
            var productConfig = await this.productConfigurationProvider.GetProductConfiguration(
                releaseContext,
                WebFormAppType.Claim);
            if (productConfig == null)
            {
                return Array.Empty<string>();
            }

            var questionMetaData = productConfig.FormDataSchema.GetQuestionMetaData();
            var questionAttachmentKeys = questionMetaData?.Where(x => x.DataType == DataType.Attachment).Select(x => x.Key);

            return questionAttachmentKeys?.ToArray();
        }
    }
}
