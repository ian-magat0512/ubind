// <copyright file="AdditionalPropertyAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using UBind.Domain.Enums;

    public class AdditionalPropertyAuthorisationService : IAdditionalPropertyAuthorisationService
    {
        private readonly IAuthorisationService authorisationService;
        private readonly IUserAuthorisationService userAuthorisationService;
        private readonly IOrganisationAuthorisationService organisationAuthorisationService;

        public AdditionalPropertyAuthorisationService(
            IAuthorisationService authorisationService,
            IUserAuthorisationService userAuthorisationService,
            IOrganisationAuthorisationService organisationAuthorisationService)
        {
            this.authorisationService = authorisationService;
            this.userAuthorisationService = userAuthorisationService;
            this.organisationAuthorisationService = organisationAuthorisationService;
        }

        public async Task ThrowIfUserCannotModifyEntity(
            ClaimsPrincipal performingUser,
            Guid tenantId,
            AdditionalPropertyEntityType entityType,
            Guid entityId)
        {
            string action = $"modify additional property for entity of type \"{entityType}\" with id \"{entityId}\"";
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantId, performingUser, action);
            switch (entityType)
            {
                case AdditionalPropertyEntityType.Customer:
                    await this.authorisationService.ThrowIfUserCannotModifyCustomer(tenantId, performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Quote:
                case AdditionalPropertyEntityType.NewBusinessQuote:
                case AdditionalPropertyEntityType.AdjustmentQuote:
                case AdditionalPropertyEntityType.RenewalQuote:
                case AdditionalPropertyEntityType.CancellationQuote:
                    await this.authorisationService.ThrowIfUserCannotModifyQuote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.QuoteVersion:
                    await this.authorisationService.ThrowIfUserCannotModifyQuoteVersion(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Policy:
                    await this.authorisationService.ThrowIfUserCannotModifyPolicy(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.PolicyTransaction:
                case AdditionalPropertyEntityType.NewBusinessPolicyTransaction:
                case AdditionalPropertyEntityType.AdjustmentPolicyTransaction:
                case AdditionalPropertyEntityType.RenewalPolicyTransaction:
                case AdditionalPropertyEntityType.CancellationPolicyTransaction:
                    await this.authorisationService.ThrowIfUserCannotModifyPolicyTransaction(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Claim:
                    await this.authorisationService.ThrowIfUserCannotModifyClaim(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.ClaimVersion:
                    await this.authorisationService.ThrowIfUserCannotModifyClaimVersion(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Tenant:
                    await this.authorisationService.ThrowIfUserCannotModifyTenant(entityId, performingUser);
                    break;
                case AdditionalPropertyEntityType.Organisation:
                    await this.organisationAuthorisationService.ThrowIfUserCannotModify(tenantId, entityId, performingUser);
                    break;
                case AdditionalPropertyEntityType.Invoice:
                    await this.authorisationService.ThrowIfUserCannotModifyInvoice(performingUser, entityId);
                    break;
                /* UNCOMMENT IN ACCOUNTING EPIC */
                /*
                case AdditionalPropertyEntityType.Bill:
                    this.ThrowIfUserCannotModifyBill(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.CreditNote:
                    this.ThrowIfUserCannotModifyCreditNote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.DebitNote:
                    this.ThrowIfUserCannotModifyDebitNote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.CreditPayment:
                    this.ThrowIfUserCannotModifyCreditPayment(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.DebitPayment:
                    this.ThrowIfUserCannotModifyDebitPayment(performingUser, entityId);
                    break;
                */
                case AdditionalPropertyEntityType.Product:
                    await this.authorisationService.ThrowIfUserCannotModifyProduct(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Portal:
                    await this.authorisationService.ThrowIfUserCannotModifyPortal(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.User:
                    await this.userAuthorisationService.ThrowIfUserCannotModify(tenantId, entityId, performingUser);
                    break;
                default:
                    throw new InvalidOperationException("When processing the modification of an additional property "
                        + $"value, we were trying to check authorisation to modify an entity of type {entityType} "
                        + "but that entity type does not have any authorisation handling yet.");
            }
        }

        public async Task ThrowIfUserCannotViewEntity(
            ClaimsPrincipal performingUser,
            Guid tenantId,
            AdditionalPropertyEntityType entityType,
            Guid? entityId)
        {
            string action = $"view additional property for entity of type \"{entityType}\" with id \"{entityId}\"";
            await this.authorisationService.ThrowIfUserCannotAccessTenant(tenantId, performingUser, action);
            switch (entityType)
            {
                case AdditionalPropertyEntityType.Customer:
                    await this.authorisationService.ThrowIfUserCannotViewCustomer(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Quote:
                case AdditionalPropertyEntityType.NewBusinessQuote:
                case AdditionalPropertyEntityType.AdjustmentQuote:
                case AdditionalPropertyEntityType.RenewalQuote:
                case AdditionalPropertyEntityType.CancellationQuote:
                    await this.authorisationService.ThrowIfUserCannotViewQuote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.QuoteVersion:
                    await this.authorisationService.ThrowIfUserCannotViewQuoteVersion(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Policy:
                    await this.authorisationService.ThrowIfUserCannotViewPolicy(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.PolicyTransaction:
                case AdditionalPropertyEntityType.NewBusinessPolicyTransaction:
                case AdditionalPropertyEntityType.AdjustmentPolicyTransaction:
                case AdditionalPropertyEntityType.RenewalPolicyTransaction:
                case AdditionalPropertyEntityType.CancellationPolicyTransaction:
                    await this.authorisationService.ThrowIfUserCannotViewPolicyTransaction(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Claim:
                    await this.authorisationService.ThrowIfUserCannotViewClaim(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.ClaimVersion:
                    await this.authorisationService.ThrowIfUserCannotViewClaimVersion(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Tenant:
                    await this.authorisationService.ThrowIfUserCannotViewTenant(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Organisation:
                    await this.organisationAuthorisationService.ThrowIfUserCannotView(tenantId, entityId.Value, performingUser);
                    break;
                case AdditionalPropertyEntityType.Invoice:
                    await this.authorisationService.ThrowIfUserCannotViewInvoice(performingUser, entityId);
                    break;
                /* UNCOMMENT IN ACCOUNTING EPIC */
                /*
                case AdditionalPropertyEntityType.Bill:
                    this.ThrowIfUserCannotViewBill(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.CreditNote:
                    this.ThrowIfUserCannotViewCreditNote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.DebitNote:
                    this.ThrowIfUserCannotViewDebitNote(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.CreditPayment:
                    this.ThrowIfUserCannotViewCreditPayment(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.DebitPayment:
                    this.ThrowIfUserCannotViewDebitPayment(performingUser, entityId);
                    break;
                */
                case AdditionalPropertyEntityType.Product:
                    await this.authorisationService.ThrowIfUserCannotViewProduct(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.Portal:
                    await this.authorisationService.ThrowIfUserCannotViewPortal(performingUser, entityId);
                    break;
                case AdditionalPropertyEntityType.User:
                    await this.userAuthorisationService.ThrowIfUserCannotView(tenantId, entityId.Value, performingUser);
                    break;
                case AdditionalPropertyEntityType.None:
                    break;
                default:
                    throw new InvalidOperationException("When retreiving an additional property "
                        + $"value, we were trying to check authorisation to view an entity of type {entityType} "
                        + "but that entity type does not have any authorisation handling yet.");
            }
        }

        public async Task ThrowIfUserCannotViewEntityType(
            ClaimsPrincipal performingUser,
            Guid tenantId,
            AdditionalPropertyEntityType entityType)
        {
            switch (entityType)
            {
                case AdditionalPropertyEntityType.User:
                    await this.userAuthorisationService.ThrowIfUserCannotViewAny(performingUser);
                    return;
                case AdditionalPropertyEntityType.Organisation:
                    await this.organisationAuthorisationService.ThrowIfUserCannotViewAny(performingUser);
                    return;
            }

            await this.ThrowIfUserCannotViewEntity(performingUser, tenantId, entityType, null);
        }
    }
}
