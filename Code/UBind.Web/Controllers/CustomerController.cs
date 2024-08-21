// <copyright file="CustomerController.cs" company="uBind">
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
    using CSharpFunctionalExtensions;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Customer.Merge;
    using UBind.Application.Commands.Person;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Person;
    using UBind.Application.Queries.Accounting;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Person;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling portal-related customer services.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/customer")]
    public class CustomerController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICustomerService customerService;
        private readonly IQuoteService quoteService;
        private readonly IPersonService personService;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IClaimService claimService;
        private readonly IClock clock;
        private readonly ITenantService tenantService;
        private readonly IAuthorisationService authorisationService;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="personService">The person service.</param>
        /// <param name="policyReadModelRepository">The policy reaed model repository.</param>
        /// <param name="customerReadModelRespository">The customer read model repository.</param>
        /// <param name="claimService">The claim service.</param>
        /// <param name="clock">A clock.</param>
        /// <param name="tenantService">The tenant service.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="additionalPropertyValueService">Additional property value service.</param>
        public CustomerController(
            ICachingResolver cachingResolver,
            ICustomerService customerService,
            IQuoteService quoteService,
            IPersonService personService,
            IPolicyReadModelRepository policyReadModelRepository,
            ICustomerReadModelRepository customerReadModelRespository,
            IClaimService claimService,
            IClock clock,
            ITenantService tenantService,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            IAdditionalPropertyValueService additionalPropertyValueService)
        {
            this.cachingResolver = cachingResolver;
            this.customerService = customerService;
            this.quoteService = quoteService;
            this.personService = personService;
            this.policyReadModelRepository = policyReadModelRepository;
            this.customerReadModelRepository = customerReadModelRespository;
            this.claimService = claimService;
            this.clock = clock;
            this.tenantService = tenantService;
            this.mediator = mediator;
            this.authorisationService = authorisationService;
            this.additionalPropertyValueService = additionalPropertyValueService;
        }

        /// <summary>
        /// Gets the list of customers for a given user (either those owned by the user, or all customers for admins).
        /// </summary>
        /// <param name="options">The additional query options to be used.</param>
        /// <returns>OK.</returns>
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpGet]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(IEnumerable<CustomerSetModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomers([FromQuery] QueryOptionsModel options)
        {
            var userTenantId = this.User.GetTenantId();
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var filters = await options.ToFilters(userTenantId, this.cachingResolver, nameof(CustomerReadModel.CreatedTicksSinceEpoch));

            await this.authorisationService.ApplyViewCustomerRestrictionsToFilters(this.User, filters);

            var customersMatchingFilterQuery = new GetCustomersMatchingFiltersQuery(userTenantId, filters);
            var customers = await this.mediator.Send(customersMatchingFilterQuery);
            var model = customers.Select(customer => new CustomerSetModel(customer));
            return this.Ok(model);
        }

        /// <summary>
        /// Gets a customer by id.
        /// </summary>
        /// <param name="customerId">the id of the customer.</param>
        /// <param name="environment">the environment from which the request is for. This is optional.</param>
        /// <returns>OK.</returns>
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [HttpGet]
        [Route("{customerId}")]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerById(Guid customerId, [FromQuery] string environment = null)
        {
            var customerDetailsQuery = new GetCustomerSummaryByIdQuery(this.User.GetTenantId(), customerId);
            var customerSummary = await this.mediator.Send(customerDetailsQuery);
            EntityHelper.ThrowIfNotFoundOrCouldHaveBeenDeleted(customerSummary, customerId, "Customer");
            await this.authorisationService.ThrowIfUserCannotViewCustomer(this.User, customerSummary);
            if (environment.IsNotNullOrEmpty())
            {
                var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(this.User.GetTenantId());
                var isMutual = TenantHelper.IsMutual(tenantAlias);
            }

            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Gets the details of a specific customer record based on the ID given.
        /// </summary>
        /// <param name="customerId">The ID of the customer the caller is requesting.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustBeLoggedIn]
        [Route("{customerId}/detail")]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(CustomerDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomerDetails(Guid customerId, [FromQuery] DeploymentEnvironment? environment = null)
        {
            var customerReadModel = this.customerReadModelRepository.GetCustomerById(this.User.GetTenantId(), customerId);

            EntityHelper.ThrowIfNotFoundOrCouldHaveBeenDeleted(customerReadModel, customerId, "Customer");
            await this.authorisationService.ThrowIfUserCannotViewCustomer(this.User, customerReadModel);

            QuoteReadModelFilters quoteFilters = new QuoteReadModelFilters
            {
                TenantId = customerReadModel.TenantId,
                OrganisationIds = new List<Guid> { customerReadModel.OrganisationId },
                Environment = environment,
                CustomerId = customerReadModel.Id,
                ExcludedStatuses = new List<string>() { StandardQuoteStates.Nascent },
            };

            IEnumerable<IQuoteReadModelSummary> quotes = this.quoteService.GetQuotes(
                customerReadModel.TenantId,
                quoteFilters);

            var policyFilters = new PolicyReadModelFilters
            {
                TenantId = customerReadModel.TenantId,
                OrganisationIds = new List<Guid> { customerReadModel.OrganisationId },
                Environment = environment,
                CustomerId = customerReadModel.Id,
            };
            IEnumerable<IPolicyReadModelSummary> policies =
                this.policyReadModelRepository.ListPolicies(customerReadModel.TenantId, policyFilters);

            var claimFilters = new EntityListFilters
            {
                TenantId = customerReadModel.TenantId,
                OrganisationIds = new List<Guid> { customerReadModel.OrganisationId },
                Environment = environment,
                CustomerId = customerReadModel.Id,
                SortBy = nameof(ClaimReadModel.CreatedTicksSinceEpoch),
                SortOrder = SortDirection.Descending,
            };
            var claims = this.claimService.GetClaims(customerReadModel.TenantId, claimFilters);

            var paymentsQuery = new AllPaymentsByPayerQuery(customerId, TransactionPartyType.Customer);
            var payments = await this.mediator.Send(paymentsQuery);

            var refundsQuery = new AllRefundsByPayerQuery(customerId, TransactionPartyType.Customer);
            var refunds = await this.mediator.Send(refundsQuery);
            var person = this.personService.Get(customerReadModel.TenantId, customerReadModel.PrimaryPersonId);
            var additionalPropertyValuesDto = await this.additionalPropertyValueService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                customerReadModel.TenantId,
                Domain.Enums.AdditionalPropertyEntityType.Customer,
                customerId);

            PortalReadModel portal = null;
            if (customerReadModel.PortalId.HasValue)
            {
                portal = await this.mediator.Send(
                    new GetPortalByIdQuery(customerReadModel.TenantId, customerReadModel.PortalId.Value));
            }

            var people = this.customerService.GetPersonsForCustomer(customerReadModel.TenantId, customerReadModel.Id);
            var model = new CustomerDetailsModel(
                customerReadModel,
                new CustomerRecordsModel(quotes, claims, policies, payments, refunds, person),
                people,
                portal,
                this.clock.Now(),
                additionalPropertyValuesDto);

            return this.Ok(model);
        }

        /// <summary>
        /// A request that creates an independent customer record (Not a customer user nor owned by any user).
        /// </summary>
        /// <param name="environment">The environment to use.</param>
        /// <param name="model">The details for the new customer.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCustomer(
            [FromQuery] string environment,
            [FromBody] CustomerPersonalDetailsModel model)
        {
            DeploymentEnvironment env = EnvironmentHelper.ParseEnvironmentOrThrow(environment);
            var userTenantId = this.User.GetTenantId();
            ResolvedCustomerPersonalDetailsModel resolvedCustomerPersonalDetailsModel
                = await this.CreateResolvedCustomerPersonalDetailsModel(model);
            var portalId = model.Portal.IsNullOrEmpty() ? model.PortalId : model.Portal;
            var portalModel = await this.cachingResolver.GetPortalOrNull(userTenantId, new GuidOrAlias(portalId));
            var createCustomerCommand = new CreateCustomerCommand(
                userTenantId,
                env,
                resolvedCustomerPersonalDetailsModel,
                this.User.GetId(),
                portalModel?.Id,
                additionalProperties: model.Properties.ToDomainAdditionalProperties());
            var customerId = await this.mediator.Send(createCustomerCommand);
            var customerDetailsQuery = new GetCustomerSummaryByIdQuery(userTenantId, customerId);
            var customerSummary = await this.mediator.Send(customerDetailsQuery);
            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// A request that merges the entities of the source customer to the target customer, deleting the source.
        /// </summary>
        /// <param name="sourceCustomerId">The source customer Id where the data will be the source of the merge.
        /// will then be deleted afterwards.</param>
        /// <param name="targetCustomerId">The target where the contents of the source customer will go to.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [Route("{sourceCustomerId}/merge-with/{targetCustomerId}")]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(CustomerMergeSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> MergeWith(Guid sourceCustomerId, Guid targetCustomerId, Guid? tenantId = null)
        {
            tenantId = this.GetContextTenantIdOrThrow(tenantId);
            var mergeCommand = new MergeSourceToTargetCustomerCommand(tenantId.Value, sourceCustomerId, targetCustomerId, this.User.GetId());
            await this.mediator.Send(mergeCommand);
            return this.Ok(
                new CustomerMergeSetModel(
                    mergeCommand.SourceCustomerDisplayName,
                    mergeCommand.TargetCustomerDisplayName,
                    targetCustomerId));
        }

        /// <summary>
        /// Creates a new customer, with a primary person, and a user account for that person.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <param name="model">The details for the new customer.</param>
        /// <returns>Ok.</returns>
        [HttpPost("create-with-account")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCustomerAccount(
            [FromQuery] DeploymentEnvironment environment,
            [FromBody] CreateCustomerAccountModel model)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(model.Tenant));
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, new GuidOrAlias(model.Organisation));
            var portalGuidOrAlias = new GuidOrAlias(model.Portal);
            PortalReadModel? portal = await this.cachingResolver.GetPortalOrThrow(tenant.Id, portalGuidOrAlias);
            var entitySettings = await this.mediator.Send(
                new GetPortalEntitySettingsQuery(tenant.Id, portal.Id));
            if (entitySettings == null || !entitySettings.AllowCustomerSelfAccountCreation)
            {
                throw new ErrorException(Errors.Portal.SelfRegistrationNotAllowed(portal.Name, organisation.Name));
            }

            var command = new CreateCustomerPersonUserAccountCommand(
                tenant.Id,
                portal.Id,
                environment,
                model,
                organisation.Id);
            var personId = await this.mediator.Send(command);
            var customerSummaryCommand = new GetCustomerSummaryByPersonIdQuery(tenant.Id, personId);
            var customerSummary = await this.mediator.Send(customerSummaryCommand);
            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Handle customer update requests.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer to update.</param>
        /// <param name="model">The details for the new customer.</param>
        /// <returns>Ok.</returns>
        [HttpPut]
        [Route("{customerId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustBeLoggedIn]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCustomer(Guid customerId, [FromBody] CustomerPersonalDetailsModel model)
        {
            var tenantId = this.User.GetTenantId();
            var portalId = model.Portal.IsNullOrEmpty() ? model.PortalId : model.Portal;
            var portalModel = await this.cachingResolver.GetPortalOrNull(tenantId, new GuidOrAlias(portalId));
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(tenantId, this.User, customerId);
            var updateCustomerDetailsCommand = new UpdateCustomerDetailsCommand(
                tenantId,
                customerId,
                await this.CreateResolvedCustomerPersonalDetailsModel(model),
                portalModel?.Id,
                model.Properties.ToDomainAdditionalProperties());
            await this.mediator.Send(updateCustomerDetailsCommand);
            var customerDetailsQuery = new GetCustomerSummaryByIdQuery(tenantId, customerId);
            var customerSummary = await this.mediator.Send(customerDetailsQuery);
            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Returns true if the customer with the given id has an account. Returns false if the customer Id is unknown,
        /// or if the customer does not have an account.
        /// </summary>
        /// <param name="customerId">The Id of the customer.</param>
        /// <returns>true if the customer with the given id has an account, otherwise false.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [Route("{customerId}/has-account")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<JsonResult> HasAccount(Guid customerId, [FromQuery] string tenant)
        {
            tenant = tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var customerHasUserAccountQuery = new CustomerHasUserAccountQuery(tenantModel.Id, customerId);
            var hasUserAccount = await this.mediator.Send(customerHasUserAccountQuery);
            return this.Json(hasUserAccount);
        }

        /// <summary>
        /// Assign owner user to a customer by owner user id.
        /// Passing a null value to owner user id will unassign the owner.
        /// </summary>
        /// <param name="customerId">The customer id.</param>
        /// <param name="ownerUserId">The owner user id.</param>
        /// <returns>The updated customer.</returns>
        [MustBeLoggedIn]
        [HttpPatch]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [Route("{customerId}/owner")]
        public async Task<IActionResult> AssignOwner(Guid customerId, Guid? ownerUserId)
        {
            var command = new AssignOwnerCommand(this.User.GetTenantId(), customerId, ownerUserId);
            await this.mediator.Send(command);
            return this.NoContent();
        }

        /// <summary>
        /// Gets the details of the primary person record based on the ID given.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [Route("{customerId}/person/primary")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(PersonDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrimaryPersonDetails(Guid customerId)
        {
            var query = new GetPrimaryPersonForCustomerQuery(this.User.GetTenantId(), customerId);
            var person = await this.mediator.Send(query);
            if (person == null)
            {
                throw new ErrorException(Errors.Customer.PersonNotFound(customerId));
            }

            var permitted = this.User.GetUserType() == UserType.Client && ((IEntityReadModel<Guid>)person).TenantId == this.User.GetTenantId();
            if (!permitted)
            {
                return Errors.General
                    .Forbidden("get a person's details", "you don't have the required permission or ownership")
                    .ToProblemJsonResult();
            }

            return this.Ok(new PersonDetailsModel(person));
        }

        /// <summary>
        /// Set the primary person for customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="customerPatchModel">The customer patch model.</param>
        /// <returns>Ok.</returns>
        [HttpPatch]
        [Route("{customerId}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(PersonDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetPersonAsPrimaryRecordForCustomer(
            Guid customerId, [FromBody] CustomerPatchModel customerPatchModel)
        {
            // Set the person as a primary for customer
            var setPrimaryPersonCommand = new SetPersonAsPrimaryForCustomerCommand(
                this.User.GetTenantId(), customerId, customerPatchModel.PrimaryPersonId);
            await this.mediator.Send(setPrimaryPersonCommand);

            // Fetch the person details
            var query = new GetPersonSummaryByIdQuery(this.User.GetTenantId(), customerPatchModel.PrimaryPersonId);
            var person = await this.mediator.Send(query);
            return this.Ok(new PersonDetailsModel(person));
        }

        /// <summary>
        /// Sets portal for a customer.
        /// </summary>
        /// <param name="customerId">The Id of the customer.</param>
        /// <param name="portalId">The Id of the portal to put into customer.</param>
        /// <returns>No content.</returns>
        [HttpPatch]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [Route("{customerId}/portal/{portalId}")]
        public async Task<IActionResult> AssignPortalToCustomer(Guid customerId, Guid portalId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(this.User.GetTenantId(), this.User, customerId);
            var assignPortalToCustomerCommand = new AssignPortalToCustomerCommand(this.User.GetTenantId(), customerId, portalId);
            await this.mediator.Send(assignPortalToCustomerCommand);
            return this.NoContent();
        }

        /// <summary>
        /// Restore deleted customer.
        /// </summary>
        /// <param name="customerId">The Id of the customer.</param>
        /// <returns>No content.</returns>
        [HttpPatch]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [Route("{customerId}/restore")]
        public async Task<IActionResult> RestoreDeletedCustomer(Guid customerId)
        {
            var command = new RestoreDeletedCustomerCommand(this.User.GetTenantId(), customerId);
            await this.mediator.Send(command);
            return this.NoContent();
        }

        /// <summary>
        /// Remove the existing portal for a customer.
        /// </summary>
        /// <param name="customerId">The Id of the customer.</param>
        /// <returns>No content.</returns>
        [HttpDelete]
        [MustBeLoggedIn]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 30)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [Route("{customerId}/portal")]
        public async Task<IActionResult> UnassignPortalFromCustomer(Guid customerId)
        {
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(this.User.GetTenantId(), this.User, customerId);
            var unassignPortalFromCustomerCommand = new UnassignPortalFromCustomerCommand(this.User.GetTenantId(), customerId);
            await this.mediator.Send(unassignPortalFromCustomerCommand);
            return this.NoContent();
        }

        /// <summary>
        /// Transfer the customer to another organisation of the same tenancy.
        /// </summary>
        /// <param name="customerId">The Id of the customer.</param>
        /// <param name="organisation">The Id of the organisation.</param>
        /// <returns>The action result containing status codes.</returns>
        [HttpPatch("{customerId}/organisation/{organisation}")]
        [RequestRateLimit(Period = 30, Type = RateLimitPeriodType.Seconds, Limit = 15)]
        [MustHaveOneOfPermissions(
            Permission.ManageCustomers,
            Permission.ManageAllCustomers,
            Permission.ManageAllCustomersForAllOrganisations,
            Permission.ManageAllOrganisations,
            Permission.ManageOrganisations)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferCustomerOrganisation(Guid customerId, string organisation)
        {
            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(this.User.GetTenantId(), new GuidOrAlias(organisation));
            await this.authorisationService.ThrowIfOrganisationIsNotInTenancy(this.User.GetTenantId(), organisationModel.Id);

            var command = new TransferCustomerToOtherOrganisationCommand(this.User.GetTenantId(), customerId, organisationModel.Id);
            await this.mediator.Send(command);

            return this.Ok($"Validation passed and now starting to transfer customer '{customerId}' to organisation '{organisation}'");
        }

        /// <summary>
        /// Retrieves the tenant id and comparing it to the users tenant or throw an error.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="errorActionMessage">Custom action message when an error is thrown.</param>
        /// <param name="errorReasonMessage">Custom error message when an error is thrown.</param>
        /// <returns>The resulting tenant Id.</returns>
        protected Guid GetContextTenantIdOrThrow(
            Guid? tenantId,
            string errorActionMessage = "access record from a different tenancy.",
            string errorReasonMessage = "")
        {
            var userTenantId = this.User.GetTenantId();
            tenantId = tenantId ?? userTenantId;
            if (tenantId != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden(errorActionMessage, errorReasonMessage));
            }

            return tenantId.Value;
        }

        private async Task<ResolvedCustomerPersonalDetailsModel> CreateResolvedCustomerPersonalDetailsModel(
            CustomerPersonalDetailsModel customerPersonalDetailsModel)
        {
            ResolvedCustomerPersonalDetailsModel resolvedCustomerPersonalDetailsModel = null;
            if (customerPersonalDetailsModel != null)
            {
                var userTenantId = this.User.GetTenantId();
                var tenant = this.tenantService.GetTenant(userTenantId);
                string organisationIdOrAlias = string.Empty;
                if (customerPersonalDetailsModel != null)
                {
                    organisationIdOrAlias =
                        !customerPersonalDetailsModel.Organisation.IsNullOrEmpty()
                    ? customerPersonalDetailsModel.Organisation
                    : customerPersonalDetailsModel.OrganisationId;
                }

                if (organisationIdOrAlias.IsNullOrEmpty())
                {
                    organisationIdOrAlias = tenant.Details.DefaultOrganisationId.ToString();
                }

                var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, new GuidOrAlias(organisationIdOrAlias));
                var userOrganisationId = this.User.GetOrganisationId();

                var portalIdOrAlias = customerPersonalDetailsModel.Portal.IsNullOrEmpty()
                    ? customerPersonalDetailsModel.PortalId
                    : customerPersonalDetailsModel.Portal;
                if (!string.IsNullOrEmpty(portalIdOrAlias))
                {
                    var portal = await this.cachingResolver.GetPortalOrNull(tenant.Id, new GuidOrAlias(portalIdOrAlias));
                    if (portal == null)
                    {
                        throw new ErrorException(Errors.Portal.NotFound(
                            tenant.Details.Alias,
                            portalIdOrAlias));
                    }

                    if (portal.OrganisationId != userOrganisationId)
                    {
                        throw new ErrorException(Errors.General.Forbidden(
                            "associate a customer with a portal from another organisation"));
                    }
                }

                if ((organisationModel.Id != userOrganisationId)
                    && tenant.Details.DefaultOrganisationId != userOrganisationId)
                {
                    throw new ErrorException(Errors.General.Forbidden(
                        "create or update a customer", "you don't have the required permission"));
                }

                resolvedCustomerPersonalDetailsModel
                    = new ResolvedCustomerPersonalDetailsModel(organisationModel.Id, tenant, customerPersonalDetailsModel);
            }

            return resolvedCustomerPersonalDetailsModel;
        }
    }
}
