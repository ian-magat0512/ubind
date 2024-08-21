// <copyright file="FinancialTransactionController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using NodaMoney;
    using NodaTime.Text;
    using UBind.Application.Accounting;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain;
    using UBind.Domain.Accounting;
    using UBind.Domain.Accounting.Enums;
    using UBind.Domain.Commands.Accounting;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ValueTypes;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for handling financial transactions.
    /// </summary>
    [Produces("application/json")]
    [Route("api/v1/financial-transaction")]
    public class FinancialTransactionController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IPaymentReferenceNumberGenerator paymentReferenceNumberGenerator;
        private readonly IRefundReferenceNumberGenerator refundReferenceNumberGenerator;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionController"/> class.
        /// </summary>
        /// <param name="httpContextPropertiesResolver">The resolver to get userid.</param>
        /// <param name="paymentReferenceNumberGenerator">The payment reference number generator.</param>
        /// <param name="refundReferenceNumberGenerator">The refund reference number generator.</param>
        /// <param name="mediator">The mediator command pattern service container.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public FinancialTransactionController(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IPaymentReferenceNumberGenerator paymentReferenceNumberGenerator,
            IRefundReferenceNumberGenerator refundReferenceNumberGenerator,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.mediator = mediator;
            this.paymentReferenceNumberGenerator = paymentReferenceNumberGenerator;
            this.refundReferenceNumberGenerator = refundReferenceNumberGenerator;
        }

        /// <summary>
        /// Create a payment for the customer for basic accounting.
        /// </summary>
        /// <param name="options">The query options.</param>
        /// <param name="transaction">The payment/refund object.</param>
        /// <returns>The action result.</returns>
        [MustBeLoggedIn]
        [HttpPost]
        [Route("create")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [MustHaveOneOfPermissions(Permission.ManageCustomers, Permission.ManageAllCustomers, Permission.ManageAllCustomersForAllOrganisations)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateFinancialTransactionForCustomer(
            [FromQuery] QueryOptionsModel options,
            [FromBody] FinancialTransactionResourceModel transaction)
        {
            var userTenantId = this.User.GetTenantId();
            var tenant = options.Tenant ?? userTenantId.ToString();
            var tenantModel = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenant));
            if (tenantModel.Id != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                return Errors.Tenant.Mismatch("payment", tenantModel.Id, this.User.GetTenantId()).ToProblemJsonResult();
            }

            string environment = options.Environment ?? "production";
            var parseEnvironment = Enum.TryParse<DeploymentEnvironment>(environment, true, out DeploymentEnvironment env);
            if (!parseEnvironment)
            {
                return Errors.General.NotFound("environment", environment).ToProblemJsonResult();
            }

            // TODO: Once migration work on UB-4657 is done, we should remove the following 3 lines.
            if (env == DeploymentEnvironment.Production)
            {
                return Errors.General.FeatureUnderConstruction("Accounting Financial Transaction").ToProblemJsonResult();
            }

            var amount = new Money(transaction.Amount, Currency.FromCode(AccountingConstants.DefaultCurrency));

            if (transaction.Type == FinancialTransactionType.Payment)
            {
                var referenceNumber = this.paymentReferenceNumberGenerator.GeneratePaymentReferenceNumber(tenantModel.Id, env).ToString();
                var command = new CreatePaymentCommand(
                    tenantModel.Id,
                    amount,
                    InstantPattern.General.Parse(transaction.TransactionDateTime).Value,
                    new TransactionParties(transaction.CustomerId, TransactionPartyType.Customer),
                    this.httpContextPropertiesResolver.PerformingUserId,
                    referenceNumber.ToString());

                await this.mediator.Send(command);
                return this.Ok(referenceNumber.PadLeft(6, '0'));
            }
            else if (transaction.Type == FinancialTransactionType.Refund)
            {
                var referenceNumber = this.refundReferenceNumberGenerator.GenerateRefundReferenceNumber(tenantModel.Id, env).ToString();

                var command = new CreateRefundCommand(
                   tenantModel.Id,
                   amount,
                   InstantPattern.General.Parse(transaction.TransactionDateTime).Value,
                   new TransactionParties(transaction.CustomerId, TransactionPartyType.Customer),
                   this.httpContextPropertiesResolver.PerformingUserId,
                   referenceNumber);

                await this.mediator.Send(command);
                return this.Ok(referenceNumber.PadLeft(6, '0'));
            }
            else
            {
                return Errors.General.BadRequest($"the financial transaction type: {transaction.Type} is invalid.").ToProblemJsonResult();
            }
        }
    }
}
