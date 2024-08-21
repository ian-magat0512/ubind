// <copyright file="PolicyController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using UBind.Application;
using UBind.Application.Authorisation;
using UBind.Application.Commands.Policy;
using UBind.Application.Commands.Quote;
using UBind.Application.Dashboard.Model;
using UBind.Application.ExtensionMethods;
using UBind.Application.Helpers;
using UBind.Application.Queries.Policy;
using UBind.Application.Queries.PolicyTransaction;
using UBind.Application.Queries.ProductRelease;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.Product;
using UBind.Domain.Product.Component.Form;
using UBind.Domain.ReadModel;
using UBind.Domain.Search;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.ValueTypes;
using UBind.Web.Extensions;
using UBind.Web.Filters;
using UBind.Web.ResourceModels;
using UBind.Web.ResourceModels.Policy;
using UBind.Web.ResourceModels.Quote;

/// <summary>
/// Controller for portal-related policy requests.
/// </summary>
[MustBeLoggedIn]
[Route("/api/v1/policy")]
[Produces("application/json")]
[RequiresFeature(Feature.PolicyManagement)]
public class PolicyController : Controller
{
    private readonly ICustomerService customerService;
    private readonly ICachingResolver cachingResolver;
    private readonly IPolicyService policyService;
    private readonly IClock clock;
    private readonly IConfigurationService configurationService;
    private readonly IProductConfigurationProvider productConfigurationProvider;
    private readonly IFormDataPrettifier formDataPrettifier;
    private readonly ICqrsMediator mediator;
    private readonly IAuthorisationService authorisationService;
    private readonly IAdditionalPropertyValueService additionalPropertyValueService;
    private readonly IRenewalInvitationService renewalInviationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyController"/> class.
    /// </summary>
    /// <param name="policyService">The policy service.</param>
    /// <param name="clock">A clock.</param>
    /// <param name="configurationService">The Configuration service.</param>
    /// <param name="productConfigurationProvider">The product configuration provider.</param>
    /// <param name="formDataPrettifier">The form data prettifier.</param>
    /// <param name="authorisationService">The authorisation service.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
    /// <param name="additionalPropertyValueService">Additional property value service.</param>
    /// <param name="customerService">The customer service.</param>
    /// <param name="renewalInviationService">The renewal invitation service.</param>
    public PolicyController(
        IPolicyService policyService,
        IClock clock,
        IConfigurationService configurationService,
        IProductConfigurationProvider productConfigurationProvider,
        IFormDataPrettifier formDataPrettifier,
        IAuthorisationService authorisationService,
        ICqrsMediator mediator,
        ICachingResolver cachingResolver,
        IAdditionalPropertyValueService additionalPropertyValueService,
        ICustomerService customerService,
        IRenewalInvitationService renewalInviationService)
    {
        this.customerService = customerService;
        this.cachingResolver = cachingResolver;
        this.policyService = policyService;
        this.clock = clock;
        this.configurationService = configurationService;
        this.productConfigurationProvider = productConfigurationProvider;
        this.formDataPrettifier = formDataPrettifier;
        this.authorisationService = authorisationService;
        this.mediator = mediator;
        this.additionalPropertyValueService = additionalPropertyValueService;
        this.renewalInviationService = renewalInviationService;
    }

    /// <summary>
    /// Handle policy cancellation requests.
    /// </summary>
    /// <param name="policyId">The ID of the quote/policy the cancellation is for.</param>
    /// <returns>Ok.</returns>
    [HttpPost]
    [Route("{policyId}/cancel")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(
        Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ManageAllPoliciesForAllOrganisations)]
    [ProducesResponseType(typeof(QuoteCreateResultModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid policyId)
    {
        await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, policyId);
        var cancellationQuote = await this.mediator.Send(
            new CreateCancellationQuoteCommand(this.User.GetTenantId(), policyId, false, initialQuoteState: StandardQuoteStates.Incomplete));
        return this.Ok(new QuoteCreateResultModel(cancellationQuote));
    }

    /// <summary>
    /// Creates an adjustment quote for a policy.
    /// </summary>
    /// <param name="policyId">The ID of the quote/policy to adjust.</param>
    /// <returns>Ok.</returns>
    [HttpPost]
    [Route("{policyId}/adjust")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [MustHaveOneOfPermissions(
        Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ManageAllPoliciesForAllOrganisations)]
    [ProducesResponseType(typeof(QuoteCreateResultModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Adjust(Guid policyId)
    {
        await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, policyId);
        var adjustmentQuote = await this.mediator.Send(
            new CreateAdjustmentQuoteCommand(this.User.GetTenantId(), policyId, false, initialQuoteState: StandardQuoteStates.Incomplete));
        return this.Ok(new QuoteCreateResultModel(adjustmentQuote));
    }

    /// <summary>
    /// Send Policy Renewal Invitation.
    /// </summary>
    /// <param name="policyId">The policy id to send renewal.</param>
    /// <param name="environment">The environment the policies belong to.</param>
    /// <param name="isUserAccountRequired">
    ///         A flag that checks whether a user record should be created prior to sending the renewal invitation,
    ///     if there is none.
    /// </param>
    /// <returns>The policy record.</returns>
    [MustBeLoggedIn]
    [HttpPost]
    [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
    [MustHaveOneOfPermissions(Permission.ManageMessages, Permission.ManageAllMessages)]
    [Route("{policyId}/send-renewal-invitation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SendPolicyRenewalInvitation(
        Guid policyId, [FromQuery] DeploymentEnvironment environment, [FromQuery] bool isUserAccountRequired = false)
    {
        var authenticationData = this.User.GetAuthenticationData();
        var tenantId = this.User.GetAuthenticationData().TenantId;
        var userId = (Guid)(authenticationData != null ? authenticationData.UserId : default);
        var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
        var isMutual = TenantHelper.IsMutual(tenant.Details.Alias);
        await this.renewalInviationService.SendPolicyRenewalInvitation(
            tenantId, environment, policyId, userId, isMutual, isUserAccountRequired);
        return this.Ok();
    }

    /// <summary>
    /// Creates a renewal quote for a policy.
    /// </summary>
    /// <param name="policyId">The ID of the policy to adjust.</param>
    /// <param name="environment">The environment.</param>
    /// <returns>Ok.</returns>
    [HttpPost]
    [Route("{policyId}/renew")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [MustHaveOneOfPermissions(
        Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ManageAllPoliciesForAllOrganisations)]
    [ProducesResponseType(typeof(QuoteCreateResultModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Renew(Guid policyId, DeploymentEnvironment environment)
    {
        await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, policyId);
        var renewalQuote = await this.mediator.Send(
            new CreateRenewalQuoteCommand(this.User.GetTenantId(), policyId, false, initialQuoteState: StandardQuoteStates.Incomplete));
        return this.Ok(new QuoteCreateResultModel(renewalQuote));
    }

    /// <summary>
    /// Retrieve a collection of policies based on the given parameters.
    /// </summary>
    /// <param name="options">The filter options to be used to retrieve the list.</param>
    /// <returns>A collection of policies.</returns>
    [HttpGet]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<PolicySetModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicies([FromQuery] PolicyQueryOptionsModel options)
    {
        await this.authorisationService.CheckAndStandardiseOptions(this.User, options, true);
        var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(options.Tenant));
        PolicyReadModelFilters filters = await options.ToFilters(tenantModel.Id, this.cachingResolver);
        await this.authorisationService.ApplyViewPolicyRestrictionsToFilters(this.User, filters);
        await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);
        IEnumerable<IPolicySearchResultItemReadModel> policyReadModels = await this.mediator.Send(
            new SearchPoliciesIndexQuery(options.TenantId.Value, filters.Environment.Value, filters));
        var models = policyReadModels.Select(policy => new PolicySetModel(policy));
        return this.Ok(models);
    }

    /// <summary>
    /// Gets the list of all the quotes periodic summary for dashboard.
    /// </summary>
    /// <param name="options">
    /// The required and optional filter options to be used on the request.
    /// </param>
    /// <returns>The latest collection of quotes summary available in the system.</returns>
    [HttpGet]
    [Route("periodic-summary")]
    [MustHaveUserType(UserType.Client)]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
    [MustHaveOneOfPermissions(Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<PolicyTransactionPeriodicSummaryModel>), StatusCodes.Status200OK)]
    [SingleValueQueryParameter(
            nameof(BasePeriodicSummaryQueryOptionsModel.Environment),
            nameof(BasePeriodicSummaryQueryOptionsModel.SamplePeriodLength),
            nameof(BasePeriodicSummaryQueryOptionsModel.FromDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.ToDateTime),
            nameof(BasePeriodicSummaryQueryOptionsModel.CustomSamplePeriodMinutes),
            nameof(BasePeriodicSummaryQueryOptionsModel.Timezone))]
    public async Task<IActionResult> GetPeriodicSummary([FromQuery] PolicyTransactionPeriodicSummaryQueryOptionsModel options)
    {
        var tenantId = this.User.GetTenantId();
        options.ValidateQueryOptions();
        await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
        PolicyReadModelFilters filters = await options.ToFilters(tenantId, this.cachingResolver);
        await this.authorisationService.ApplyViewPolicyRestrictionsToFilters(this.User, filters);
        await this.authorisationService.ApplyViewQuoteRestrictionsToFiltersForRideProtect(this.User, filters);
        var policyTransactionSummaries = await this.mediator.Send(new GetPolicyTransactionPeriodicSummariesQuery(tenantId, filters, options));
        if (options.SamplePeriodLength == SamplePeriodLength.All && policyTransactionSummaries.Any())
        {
            return this.Ok(policyTransactionSummaries.First());
        }

        return this.Ok(policyTransactionSummaries);
    }

    /// <summary>
    /// Retrieve a single policy's high level details.
    /// </summary>
    /// <param name="policyId">The policy id.</param>
    /// <param name="environment">The environment the policies belong to.</param>
    /// <returns>A single policy.</returns>
    [HttpGet]
    [Route("{policyId}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies,
        Permission.ViewAllPolicies,
        Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(PolicyModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicy(Guid policyId, [FromQuery] string environment)
    {
        var getPolicyByIdQuery = new GetPolicySummaryByIdQuery(this.User.GetTenantId(), policyId);
        var policyReadModelSummary = await this.mediator.Send(getPolicyByIdQuery);

        await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policyReadModelSummary);
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
        var isMutual = TenantHelper.IsMutual(tenantAlias);
        EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(
            policyReadModelSummary.Environment,
            environment,
            TenantHelper.CheckAndChangeTextToMutual("policy", isMutual));

        var model = new PolicyModel(policyReadModelSummary, this.clock.Now());
        return this.Ok(model);
    }

    /// <summary>
    /// Deletes an entire policy aggregate with all its associated records.
    /// </summary>
    /// <param name="policyId">The ID of the policy that will be deleted.</param>
    /// <param name="deleteOrphanedCustomers">Indicates whether to delete customers that will no longer have associated quote, policy, and claim.</param>
    /// <param name="associatedClaimAction">The action to be performed on claims associated with the deleted policy.</param>
    /// <param name="reusePolicyNumber">Indicates whether the policy number associated with the deleted policy should be added to the policy number pool.</param>
    /// <param name="reuseClaimNumbers">Indicates whether the claim number associated deleted claims should be added to the claim number pool.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpDelete]
    [Route("{policyId}")]
    [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
    [MustHaveOneOfPermissions(Permission.ImportPolicies)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePolicy(
        Guid policyId,
        bool deleteOrphanedCustomers = true,
        DeletedPolicyClaimsActionType associatedClaimAction = DeletedPolicyClaimsActionType.Error,
        bool reusePolicyNumber = false,
        bool reuseClaimNumbers = false)
    {
        var command = new DeletePolicyCommand(
            this.User,
            policyId,
            deleteOrphanedCustomers,
            associatedClaimAction,
            reusePolicyNumber,
            reuseClaimNumbers);
        await this.mediator.Send(command);
        return this.Ok();
    }

    /// <summary>
    /// Retrieve a collection of policies that is for renewal.
    /// </summary>
    /// <param name="options">The filter options to be used to retrieve the list.</param>
    /// <returns>A collection of policies.</returns>
    [HttpGet]
    [Route("for-renewal")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<PolicySetModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPoliciesForRenewal([FromQuery] PolicyQueryOptionsModel options)
    {
        await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
        var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(options.Tenant));
        PolicyReadModelFilters filters = await options.ToFilters(tenantModel.Id, this.cachingResolver);
        filters.IncludeProductFeatureSetting = true;
        await this.authorisationService.ApplyViewPolicyRestrictionsToFilters(this.User, filters);
        IEnumerable<IPolicyReadModelSummary> policyReadModels = await this.mediator.Send(
            new PolicyReadModelSummariesQuery(tenantModel.Id, filters));
        var models = policyReadModels.Select(policy => new PolicySetModel(policy, this.clock.Now()));
        var policiesForRenewals = models
            .Where(p => p.IsForRenewal)
            .OrderBy(pol => pol.ExpiryDateTime)
            .ThenBy(p => p.ProductName)
            .ThenBy(x => x.PolicyNumber);
        return this.Ok(policiesForRenewals);
    }

    /// <summary>
    /// Retrieve a collection of policy transactions based on the given parameters.
    /// </summary>
    /// <param name="policyId">The policy Number.</param>
    /// <param name="environment">The environment the policies belong to.</param>
    /// <returns>A collection of policies.</returns>
    [HttpGet]
    [Route("{policyId}/history")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(IEnumerable<PolicyHistorySetModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(Guid policyId, [FromQuery] string environment)
    {
        IPolicyReadModelDetails policyDetails = await this.policyService.GetPolicy(this.User.GetTenantId(), policyId);
        await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policyDetails);
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
        var isMutual = TenantHelper.IsMutual(tenantAlias);
        EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(
            policyDetails.Environment,
            environment,
            TenantHelper.CheckAndChangeTextToMutual("policy", isMutual));
        IEnumerable<PolicyHistorySetModel> models = policyDetails.Transactions
            .Select(pt => new PolicyHistorySetModel(
                pt.PolicyTransaction, policyDetails.AreTimestampsAuthoritative, policyDetails.TimeZone, policyDetails.PolicyNumber));
        return this.Ok(models);
    }

    /// <summary>
    /// Gets a policy and subsequent application record using the parameter given.
    /// </summary>
    /// <param name="policyId">The parameter to be used to retrieve the policy record.</param>
    /// <param name="type">The Type of Details.</param>
    /// <param name="environment">The environment the policies belong to.</param>
    /// <returns>The policy record.</returns>
    [HttpGet]
    [Route("{policyId}/detail/{type}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(PolicyDetailModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicyDetails(Guid policyId, string type, [FromQuery] string environment)
    {
        IPolicyReadModelDetails policyDetails = await this.policyService.GetPolicy(this.User.GetTenantId(), policyId);
        if (policyDetails == null)
        {
            return Errors.Policy.NotFound(policyId).ToProblemJsonResult();
        }

        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
        var isMutual = TenantHelper.IsMutual(tenantAlias);
        await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policyDetails);
        EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(
            policyDetails.Environment,
            environment,
            TenantHelper.CheckAndChangeTextToMutual("policy", isMutual));

        var hasType = Enum.TryParse(type, true, out PolicyDetailsType detailType);
        if (!hasType)
        {
            detailType = PolicyDetailsType.Base;
        }

        Guid productReleaseId = policyDetails.DisplayTransactionReleaseId
            ?? await this.mediator.Send(new GetDefaultProductReleaseIdOrThrowQuery(
                policyDetails.TenantId,
                policyDetails.ProductId,
                policyDetails.Environment));
        var releaseContext = new ReleaseContext(policyDetails.TenantId, productReleaseId, policyDetails.Environment, productReleaseId);
        var displayableFields =
            await this.configurationService.GetDisplayableFieldsAsync(releaseContext);
        var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
            releaseContext,
            WebFormAppType.Quote);
        var additionalPropertyValueDtos
            = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                policyDetails.TenantId,
                Domain.Enums.AdditionalPropertyEntityType.Policy,
                policyId);
        var model = new PolicyDetailModel(
            policyDetails,
            this.clock,
            displayableFields,
            detailType,
            formDataSchema,
            this.formDataPrettifier,
            additionalPropertyValueDtos);
        var hasClaimConfiguration = this.configurationService.DoesConfigurationExist(
            releaseContext,
            WebFormAppType.Claim);
        model.HasClaimConfiguration = hasClaimConfiguration;
        return this.Ok(model);
    }

    /// <summary>
    /// Gets a policy and subsequent application record using the parameter given.
    /// </summary>
    /// <param name="policyId">The parameter to be used to retrieve the policy record.</param>
    /// <param name="transactionId">The ID of the transaction.</param>
    /// <param name="environment">The environment the policies belong to.</param>
    /// <returns>The policy record.</returns>
    [HttpGet]
    [Route("{policyId}/transaction/{transactionId}")]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies, Permission.ViewAllPolicies, Permission.ViewAllPoliciesFromAllOrganisations)]
    [ProducesResponseType(typeof(PolicyTransactionModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicyTransactionDetails(
        Guid policyId, Guid transactionId, [FromQuery] string environment)
    {
        IPolicyReadModelDetails policyDetails = await this.policyService.GetPolicy(this.User.GetTenantId(), policyId);
        await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policyDetails);
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
        var isMutual = TenantHelper.IsMutual(tenantAlias);
        EnvironmentHelper.ThrowIfEnvironmentDoesNotMatchIfPassed(
            policyDetails.Environment,
            environment,
            TenantHelper.CheckAndChangeTextToMutual("policy", isMutual));
        var transaction = policyDetails.Transactions.SingleOrDefault(t => t.PolicyTransaction.Id == transactionId);
        if (transaction == null)
        {
            return Errors.General
                .NotFound(TenantHelper.CheckAndChangeTextToMutual("policy transaction", isMutual), transactionId)
                .ToProblemJsonResult();
        }

        var entityType = AdditionalPropertyEntityTypeConverter.FromPolicyTransaction(transaction.PolicyTransaction);
        Guid? productReleaseId = transaction.PolicyTransaction.ProductReleaseId;
        if (productReleaseId == null)
        {
            var context = await this.mediator.Send(new GetDefaultProductReleaseContextOrThrowQuery(
                policyDetails.TenantId,
                policyDetails.ProductId,
                policyDetails.Environment));
            productReleaseId = context.ProductReleaseId;
        }
        var releaseContext = new ReleaseContext(
            policyDetails.TenantId,
            policyDetails.ProductId,
            policyDetails.Environment,
            productReleaseId.Value);
        var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
            releaseContext,
            WebFormAppType.Quote);
        var additionalPropertyValueDtos
            = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                policyDetails.TenantId,
                entityType,
                transactionId);
        var displayableFields = await this.configurationService.GetDisplayableFieldsAsync(
            releaseContext);
        ReleaseBase? release = null;
        if (transaction.PolicyTransaction.ProductReleaseId != null)
        {
            release = await this.mediator.Send(new GetProductReleaseWithoutAssetsQuery(releaseContext));
        }

        var productModel = await this.cachingResolver.GetProductOrThrow(policyDetails.TenantId, policyDetails.ProductId);
        var model = new PolicyTransactionModel(
            policyDetails,
            transaction.PolicyTransaction,
            transaction.Quote,
            displayableFields,
            this.clock.Now(),
            Timezones.AET,
            formDataSchema,
            this.formDataPrettifier,
            additionalPropertyValueDtos,
            productModel.Details.Alias,
            release)
        {
            QuestionAttachmentKeys = await this.GetQuestionAttachmentKeys(releaseContext),
        };

        return this.Ok(model);
    }

    /// <summary>
    /// Gets the customer of the policy.
    /// </summary>
    /// <param name="policyId">The parameter to be used to retrieve the policy record.</param>
    /// <returns>The policy record.</returns>
    [HttpGet]
    [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
    [MustHaveOneOfPermissions(
        Permission.ViewPolicies,
        Permission.ViewAllPolicies,
        Permission.ViewAllPoliciesFromAllOrganisations)]
    [Route("{policyId}/customer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicyCustomer(Guid policyId)
    {
        IPolicyReadModelSummary policy = await this.policyService.GetPolicy(this.User.GetTenantId(), policyId);
        await this.authorisationService.ThrowIfUserCannotViewPolicy(this.User, policy);
        if (policy.CustomerId.HasValue)
        {
            var customer = this.customerService.GetCustomerById(policy.TenantId, policy.CustomerId.Value);
            var model = new CustomerSetModel(customer);
            return this.Ok(model);
        }

        return this.Ok();
    }

    /// <summary>
    /// Associate an existing policy with a new customer.
    /// </summary>
    /// <param name="policyId">The Id of the policy represented as <see cref="Guid"/>.</param>
    /// <param name="customerId">The Id of the customer represented as <see cref="Guid"/>.</param>
    /// <remarks>When using this association method, meaning the quote aggregate has an issued policy.</remarks>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [MustHaveOneOfPermissions(Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ManageAllPoliciesForAllOrganisations)]
    [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers)]
    [Route("{policyId}/associate-with-customer/{customerId}")]
    public async Task<IActionResult> AssociateWithCustomer(Guid policyId, Guid customerId)
    {
        await this.authorisationService.ThrowIfUserCannotModifyPolicy(this.User, policyId);
        var command = new AssociatePolicyWithCustomerCommand(this.User.GetTenantId(), policyId, customerId);
        await this.mediator.Send(command);
        return this.Ok($"Policy '{policyId}' has been successfully associated with customer '{customerId}'");
    }

    /// <summary>
    /// Handles update policy number requests.
    /// Updates an existing entry for policy record.
    /// </summary>
    /// <param name="policyId">The ID of the policy that will have its policy number updated.</param>
    /// <param name="environment">The current environment.</param>
    /// <param name="model">The updated policy.</param>
    /// <returns>Ok.</returns>
    [HttpPatch]
    [Route("{policyId}/update-policy-number")]
    [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
    [ValidateModel]
    [MustHaveOneOfPermissions(
        Permission.ManagePolicies, Permission.ManageAllPolicies, Permission.ManageAllPoliciesForAllOrganisations)]
    [ProducesResponseType(typeof(PolicyModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePolicyNumber(
        Guid policyId,
        [FromQuery] string environment,
        [FromBody] UpdatePolicyNumberModel model)
    {
        var isDeploymentEnvironment = Enum.TryParse(environment, true, out DeploymentEnvironment deploymentEnvironment);
        if (!isDeploymentEnvironment)
        {
            return await Task.FromResult<IActionResult>(
                this.NotFound($"Environment '{environment}' cannot be found"));
        }

        var updatePolicyNumberCommand = new UpdatePolicyNumberCommand(
            this.User.GetAuthenticationData().TenantId, policyId, model.PolicyNumber, deploymentEnvironment, model.ReturnOldPolicyNumberToPool);
        var policyReadModel = await this.mediator.Send(updatePolicyNumberCommand);
        var responseModel = new PolicyModel(policyReadModel);
        return this.Ok(responseModel);
    }

    private async Task<string[]> GetQuestionAttachmentKeys(ReleaseContext releaseContext)
    {
        var productConfig = await this.productConfigurationProvider.GetProductConfiguration(
            releaseContext, WebFormAppType.Quote);
        var questionAttachmentKeys = productConfig?.FormDataSchema?.GetQuestionMetaData()
            .Where(x => x.DataType == DataType.Attachment).Select(x => x.Key);
        return questionAttachmentKeys?.ToArray() ?? Array.Empty<string>();
    }
}
