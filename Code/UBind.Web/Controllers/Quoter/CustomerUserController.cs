// <copyright file="CustomerUserController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Queries.Person;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.Mapping;
    using UBind.Web.ResourceModels.Quote;

    /// <summary>
    /// Controller for user requests.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("/api/v1/{tenant}/{environment}/{product}")]
    public class CustomerUserController : Controller
    {
        private readonly IUserService userService;
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerUserController"/> class.
        /// </summary>
        /// <param name="userService">The user service interface instance.</param>
        /// <param name="cachingResolver">The tenant and product resolver.</param>
        /// <param name="mediator">The mediator.</param>
        public CustomerUserController(
            IUserService userService,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.userService = userService;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
        }

        /// <summary>
        /// Creates a new entry of user.
        /// </summary>
        /// <param name="environment">The deployment environment the user is being created for.</param>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="product">The product ID or Alias.</param>
        /// <param name="formType">The webform app type.</param>
        /// <param name="model">User sign-up view model.</param>
        /// <returns>Instance of the created user.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [ValidateModel]
        [Route("customerUser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> CreateCustomerUser(
            string environment,
            string tenant,
            string product,
            FormType formType,
            [FromBody] QuoteFormCustomerUpdateModel model)
        {
            if (!Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env))
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            if (string.IsNullOrEmpty(model.CustomerDetails.Email) || string.IsNullOrEmpty(model.CustomerDetails.FullName))
            {
                return Errors.General.BadRequest($"User account cannot be created with incomplete details").ToProblemJsonResult();
            }

            try
            {
                Guid organisationId;
                var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
                Domain.Product.Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
                IPersonalDetails personDetails = null;
                if (model.CustomerId.HasValue)
                {
                    var getPrimaryPersonForCustomerQuery
                        = new GetPrimaryPersonForCustomerQuery(tenantModel.Id, model.CustomerId.Value);
                    personDetails = await this.mediator.Send(getPrimaryPersonForCustomerQuery);
                    if (personDetails == null)
                    {
                        return Errors.Customer.PersonNotFound(model.CustomerId.Value).ToProblemJsonResult();
                    }

                    organisationId = model.CustomerDetails.Organisation.IsNullOrEmpty()
                        ? personDetails.OrganisationId
                        : Guid.Parse(model.CustomerDetails.OrganisationId);
                }
                else
                {
                    var customerDetailsOrganisation = model.CustomerDetails.Organisation.IsNullOrEmpty() ?
                        model.CustomerDetails.OrganisationId
                        : model.CustomerDetails.Organisation;
                    if (string.IsNullOrEmpty(customerDetailsOrganisation))
                    {
                        organisationId = tenantModel.Details.DefaultOrganisationId;
                    }
                    else
                    {
                        var organisation = await this.cachingResolver.GetOrganisationOrThrow(
                            tenantModel.Id,
                            new GuidOrAlias(customerDetailsOrganisation));
                        organisationId = organisation.Id;
                    }

                    personDetails = new UserSignupModel()
                    {
                        Environment = env,
                        UserType = UserType.Customer,
                        FullName = model.CustomerDetails.FullName,
                        PreferredName = model.CustomerDetails.PreferredName ?? model.CustomerDetails.FullName,
                        MobilePhoneNumber = model.CustomerDetails.MobilePhone,
                        HomePhoneNumber = model.CustomerDetails.HomePhone,
                        Email = model.CustomerDetails.Email,
                        TenantId = tenantModel.Id,
                        OrganisationId = organisationId,
                    };
                }

                var command = new CreateCustomerPersonUserAccountCommand(
                    tenantModel.Id,
                    model.PortalId,
                    env,
                    personDetails,
                    organisationId);
                await this.mediator.Send(command);

                // Do not give different response to the duplicate email case, as that would expose a user enumeration exploit.
                return this.NoContent();
            }
            catch (DuplicateUserEmailException)
            {
                // Do not give different response to success case, as that would expose a user enumeration exploit.
                return this.NoContent();
            }
            catch (ErrorException ex)
            {
                if (ex.Error.Code == "customer.email.address.in.use.by.user"
                    || ex.Error.Code == "user.email.address.in.use.by.user")
                {
                    // Do not give different response to success case, as that would expose a user enumeration exploit.
                    return this.NoContent();
                }

                throw ex;
            }
        }
    }
}
