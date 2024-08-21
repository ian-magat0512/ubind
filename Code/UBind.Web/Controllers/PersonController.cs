// <copyright file="PersonController.cs" company="uBind">
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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.Person;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Person;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Person;

    /// <summary>
    /// Use this API controller to manage person-related transactions.
    /// </summary>
    [MustBeLoggedIn]
    [Produces("application/json")]
    [Route("api/v1/person")]
    public class PersonController : Controller
    {
        private readonly ITenantService tenantService;
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonController"/> class.
        /// </summary>
        /// <param name="tenantService">The service that handles tenant transactions.</param>
        /// <param name="authorisationService">The service that handles authorisation transactions.</param>
        /// <param name="mediator">Defines a mediator to encapsulate request/response and publishing interaction patterns.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public PersonController(
            ITenantService tenantService,
            IAuthorisationService authorisationService,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.tenantService = tenantService;
            this.authorisationService = authorisationService;
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        /// <summary>
        /// Gets the list of all people based on the given query options.
        /// </summary>
        /// <param name="options">The additional query options to be used.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(IReadOnlyList<IPersonReadModelSummary>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPeople([FromQuery] QueryOptionsModel options)
        {
            await this.authorisationService.CheckAndStandardiseOptions(this.User, options);
            var tenant = options.Tenant ?? this.User.GetTenantId().ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            var filters = await options.ToFilters(tenantModel.Id, this.cachingResolver, nameof(PersonReadModel.CreatedTicksSinceEpoch));
            await this.authorisationService.ApplyViewCustomerRestrictionsToFilters(this.User, filters);
            var query = new GetPersonsMatchingFiltersQuery(
                tenantModel.Id,
                filters);
            var people = await this.mediator.Send(query);
            var resourceModels = people.Select(p => new PersonDetailsModel(p));
            return this.Ok(resourceModels);
        }

        /// <summary>
        /// Gets the record details of a specific person based on the given Id.
        /// </summary>
        /// <param name="personId">The <see cref="Guid"/> of the person the caller is requesting.</param>
        /// <returns>OK.</returns>
        [HttpGet]
        [Route("{personId}")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [MustHaveOneOfPermissions(Permission.ViewCustomers, Permission.ViewAllCustomers, Permission.ViewAllCustomersFromAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(PersonDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonDetails(Guid personId)
        {
            var tenantId = this.User.GetTenantId();
            this.authorisationService.ThrowIfUserNotInTheSameOrMasterTenancy(tenantId, this.User);

            var personQuery = new GetPersonSummaryByIdQuery(tenantId, personId);
            var person = await this.mediator.Send(personQuery);
            return this.Ok(new PersonDetailsModel(person));
        }

        /// <summary>
        /// Creates a new person record for a specific customer.
        /// </summary>
        /// <remarks>
        /// This implementation is only intended for Customer-related use only. If you want to reuse this API for the
        /// creation of Person for Users, then you need to handle
        /// <see cref="CreatePersonForCustomerCommand.CustomerId"/>.
        /// </remarks>
        /// <param name="model">The details for the new person represented as <see cref="PersonDetailsModel"/>.</param>
        /// <returns>Ok.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(PersonDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreatePerson([FromBody] PersonUpsertModel model)
        {
            var tenant = this.tenantService.GetTenant(this.User.GetTenantId());
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(tenant.Id, this.User, model.CustomerId);
            var resolvedPersonDetailsModel = new ResolvedPersonDetailsModel(tenant, model.OrganisationId, model);
            var command = new CreatePersonForCustomerCommand(
                tenant.Id, resolvedPersonDetailsModel, model.CustomerId);
            var personId = await this.mediator.Send(command);
            return await this.GetPersonModelActionResult(tenant.Id, personId);
        }

        /// <summary>
        /// Updates the existing person record based on the given person Id.
        /// </summary>
        /// <param name="personId">The <see cref="Guid"/> of the person to be updated.</param>
        /// <param name="model">The details for the new person represented as <see cref="PersonDetailsModel"/>.</param>
        /// <returns>Ok.</returns>
        [HttpPut]
        [Route("{personId}")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(PersonDetailsModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePerson(Guid personId, [FromBody] PersonUpsertModel model)
        {
            var tenant = this.tenantService.GetTenant(this.User.GetTenantId());
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(tenant.Id, this.User, model.CustomerId);
            var resolvedPersonDetailsModel = new ResolvedPersonDetailsModel(tenant, model.OrganisationId, model);
            var command = new UpdatePersonCommand(tenant.Id, personId, resolvedPersonDetailsModel);
            await this.mediator.Send(command);
            return await this.GetPersonModelActionResult(tenant.Id, personId);
        }

        /// <summary>
        /// Creates a user account from a customer data, then send an activation invitation.
        /// </summary>
        /// <param name="personId">The unique identifier of the customer to be created.</param>
        /// <param name="environment">The environment of the customer to be created.</param>
        /// <returns>Ok.</returns>
        [MustBeLoggedIn]
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [Route("{personId}/account")]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateUserAccountForPerson(Guid personId, DeploymentEnvironment environment)
        {
            var tenantId = this.User.GetTenantId();
            var query = new GetPersonSummaryByIdQuery(tenantId, personId);
            var person = await this.mediator.Send(query);
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(tenantId, this.User, person.CustomerId.Value);
            var command = new CreateCustomerPersonUserAccountCommand(
                tenantId, null, environment, person, person.OrganisationId, person.Id);
            await this.mediator.Send(command);
            var customerSummaryQuery = new GetCustomerSummaryByIdQuery(tenantId, person.CustomerId.Value);
            var customerSummary = await this.mediator.Send(customerSummaryQuery);

            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Deactivates the user account of a customer.
        /// </summary>
        /// <param name="personId">The unique identifier of the person to block.</param>
        /// <returns>Ok.</returns>
        [MustBeLoggedIn]
        [HttpPost]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [Route("{personId}/account/block")]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Deactivate(Guid personId)
        {
            var query = new GetPersonSummaryByIdQuery(this.User.GetTenantId(), personId);
            var person = await this.mediator.Send(query);
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(query.TenantId, this.User, person.CustomerId.Value);
            var blockPersonUserCommand = new BlockPersonUserCommand(query.TenantId, person.Id);
            await this.mediator.Send(blockPersonUserCommand);
            var customerDetailsQuery = new GetCustomerSummaryByIdQuery(query.TenantId, person.CustomerId.Value);
            var customerSummary = await this.mediator.Send(customerDetailsQuery);
            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Reactivates the user account of a customer.
        /// </summary>
        /// <param name="personId">The ID of the person to unblock.</param>
        /// <returns>Ok.</returns>
        [MustBeLoggedIn]
        [HttpPost]
        [Route("{personId}/account/unblock")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(CustomerSetModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Reactivate(Guid personId)
        {
            var query = new GetPersonSummaryByIdQuery(this.User.GetTenantId(), personId);
            var person = await this.mediator.Send(query);
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(query.TenantId, this.User, person.CustomerId.Value);
            var unblockPersonUserCommand = new UnBlockPersonUserCommand(this.User.GetTenantId(), person.Id);
            await this.mediator.Send(unblockPersonUserCommand);
            var customerDetailsQuery = new GetCustomerSummaryByIdQuery(query.TenantId, person.CustomerId.Value);
            var customerSummary = await this.mediator.Send(customerDetailsQuery);
            return this.Ok(new CustomerSetModel(customerSummary));
        }

        /// <summary>
        /// Deletes the person record and its related data.
        /// </summary>
        /// <param name="personId">The <see cref="Guid"/> of the person to delete.</param>
        /// <returns>Ok.</returns>
        [HttpDelete]
        [Route("{personId}")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [RequiresFeature(Feature.CustomerManagement)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeletePerson(Guid personId)
        {
            var query = new GetPersonSummaryByIdQuery(this.User.GetTenantId(), personId);
            var person = await this.mediator.Send(query);
            await this.authorisationService.ThrowIfUserCannotModifyCustomer(query.TenantId, this.User, person.CustomerId.Value);
            var command = new DeletePersonAndRelatedUserAccountCommand(query.TenantId, personId);
            await this.mediator.Send(command);
            return this.Ok(true);
        }

        private async Task<IActionResult> GetPersonModelActionResult(Guid tenantId, Guid personId)
        {
            var query = new GetPersonSummaryByIdQuery(tenantId, personId);
            var person = await this.mediator.Send(query);
            return this.Ok(new PersonDetailsModel(person));
        }
    }
}
