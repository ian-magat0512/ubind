// <copyright file="IAuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authorisation
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using UBind.Application.Models.User;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Redis;

    /// <summary>
    /// Provides a number of common authorisation checks for the performing user.
    /// </summary>
    public interface IAuthorisationService
    {
        Task<UserAuthorisationModel> GenerateUserAuthorisationModel(
            ClaimsPrincipal performingUser,
            Tenant tenant,
            string? serializedToken = null);

        Task<UserAuthorisationModel> GenerateUserAuthorisationModel(
            UserReadModel user,
            Tenant tenant,
            string? serializedToken = null,
            UserSessionModel? userSessionModel = null);

        Task ApplyEnvironmentRestrictionToFilters(ClaimsPrincipal performingUser, EntityListFilters filters);

        Task ApplyUserTypeRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters);

        /// <summary>
        /// Throws an exeption if the tenant paramter is not passed and the user is not logged in.
        /// </summary>
        /// <param name="performingUser">The user session.</param>
        /// <param name="tenant">The string Guid or alias of the tenant, if any.</param>
        void ThrowIfUserNotLoggedInAndTenantNotSpecified(ClaimsPrincipal performingUser, string tenant);

        /// <summary>
        /// Throws an exception if the user is not in the given tenancy.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="performingUser">The performing user.</param>
        void ThrowIfUserNotInTenancy(Guid tenantId, ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user is not in the given organisation, or the default org.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="organisationId">The organisation ID.</param>
        /// <param name="tenantId">The tenant ID.</param>
        Task ThrowIfUserNotInOrganisationOrDefaultOrganisation(
            ClaimsPrincipal performingUser,
            Guid? organisationId,
            Guid? tenantId = null);

        /// <summary>
        /// Throws an exception if the organisation is not found within the given tenancy.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="organisationId">The organisation ID.</param>
        Task ThrowIfOrganisationIsNotInTenancy(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Throws an exception if the user cannot modify product.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="action">The action performed by user.</param>
        Task ThrowIfUserCannotModifyProduct(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid productId,
            string action = null);

        /// <summary>
        /// Throws an exception if the user cannot modify product.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="product">The product.</param>
        /// <param name="action">The action performed by user.</param>
        Task ThrowIfUserCannotViewProduct(
            ClaimsPrincipal performingUser, Domain.Product.Product product, string action = null);

        /// <summary>
        /// Throws an exception if the user is not master tenant.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="action">The action performed by user.</param>
        Task ThrowIfUserIsNotFromMasterTenant(
            ClaimsPrincipal performingUser,
            string action = null);

        /// <summary>
        /// Gets a value indicating whether the user is from the master tenant.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        bool IsUserFromMasterTenant(ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot assign role to user under organisation.
        /// </summary>
        Task ThrowIfRoleIsNotAssignableToUserUnderOrganisation(Guid tenantId, Guid roleId, Guid organisationId);

        /// <summary>
        /// Throws an excpetion if the user can't access the given tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="action">The action to perform.</param>
        /// <param name="entityName">The entity name.</param>
        /// <param name="entityId">The entity Id.</param>
        Task ThrowIfUserCannotAccessTenant(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            string action = null,
            string entityName = null,
            dynamic entityId = null);

        /// <summary>
        /// Throws an exception if the user can't modify the policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policyId">The policy ID.</param>
        Task ThrowIfUserCannotModifyPolicy(
            ClaimsPrincipal performingUser,
            Guid policyId);

        /// <summary>
        /// Throws an exception if the user can't view the quote.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="quoteId">The quote ID.</param>
        /// <param name="tenantId">The Id of the tenant the quote belongs to, uses the performingUser instead of null.</param>
        Task ThrowIfUserCannotViewQuote(ClaimsPrincipal performingUser, Guid? quoteId, Guid? tenantId = null);

        /// <summary>
        /// Throws an exception if the user can't view the quote version.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="quoteVersionId">The quote versionID.</param>
        /// <param name="tenantId">The Id of the tenant the quote belongs to, uses the performingUser instead of null.</param>
        Task ThrowIfUserCannotViewQuoteVersion(ClaimsPrincipal performingUser, Guid quoteVersionId, Guid? tenantId = null);

        /// <summary>
        /// Throws an exception if the user can't view the quote.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="quote">The quote.</param>
        Task ThrowIfUserCannotViewQuote(ClaimsPrincipal performingUser, IQuoteReadModelSummary quote);

        /// <summary>
        /// Throws an exception if the user can't view the claim.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claimId">The claim Id.</param>
        Task ThrowIfUserCannotViewClaim(ClaimsPrincipal performingUser, Guid? claimId);

        /// <summary>
        /// Throws an exception if the user can't view the claim version.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claimVersionId">The claim version id.</param>
        Task ThrowIfUserCannotViewClaimVersion(ClaimsPrincipal performingUser, Guid claimVersionId);

        /// <summary>
        /// Throws an exception only if the user is authenticated and can't view the claim.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claimId">The claim Id.</param>
        Task ThrowIfUserIsAuthenticatedAndCannotViewClaim(ClaimsPrincipal performingUser, Guid claimId);

        /// <summary>
        /// Throws an exception if the user can't view the claim.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claim">The claim.</param>
        Task ThrowIfUserCannotViewClaim(ClaimsPrincipal performingUser, IClaimReadModelSummary? claim);

        /// <summary>
        /// Throws an exception if the user can't view the policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policyId">The policy ID.</param>
        /// <param name="tenantId">The Id of the tenant the quote belongs to, uses the performingUser instead of null.</param>
        Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, Guid policyId, Guid? tenantId = null);

        /// <summary>
        /// Throws an exception if the user can't view the policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policy">The policy.</param>
        Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, PolicyReadModel? policy);

        /// <summary>
        /// Throws an exception if the user can't view the policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policyId">The ID of the policy.</param>
        Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, Guid? policyId);

        /// <summary>
        /// Throws an exception if the user can't view the policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policy">The policy.</param>
        Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, IPolicyReadModelSummary policy);

        /// <summary>
        /// Throws an exception if the user can't view the customer using the view customer or view user permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="customer">The customer.</param>
        Task ThrowIfUserCannotViewCustomerWithViewCustomerOrViewUserPermission(ClaimsPrincipal performingUser, ICustomerReadModelSummary customer);

        /// <summary>
        /// Throws an exception if the user can't view the customer.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="customer">The customer.</param>
        Task ThrowIfUserCannotViewCustomer(
            ClaimsPrincipal performingUser, ICustomerReadModelSummary customer);

        /// <summary>
        /// Throws an exception if the user can't view the customer.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="customerId">The customer ID.</param>
        Task ThrowIfUserCannotViewCustomer(ClaimsPrincipal performingUser, Guid? customerId);

        /// <summary>
        /// Throws an exception if the user can't view the customer.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="email">The email.</param>
        Task ThrowIfUserCannotViewEmail(ClaimsPrincipal performingUser, IEmailDetails email);

        /// <summary>
        /// Throws an exception if a user cannot modify the quote.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <param name="tenantId">The tenant Id.</param>
        Task ThrowIfUserCannotModifyQuote(
            ClaimsPrincipal performingUser,
            Guid quoteId,
            Guid? tenantId = null);

        /// <summary>
        /// Throws an exception if a user cannot modify the claim.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claimId">The claim Id.</param>
        Task ThrowIfUserCannotModifyClaim(
            ClaimsPrincipal performingUser,
            Guid claimId);

        /// <summary>
        /// Throws an exception only if a user is authenticated cannot modify the claim.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="claimId">The claim Id.</param>
        Task ThrowIfUserIsAuthenticatedAndCannotModifyClaim(
            ClaimsPrincipal performingUser,
            Guid claimId);

        /// <summary>
        /// Throws an exception if a customer is not modifyable the user.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="customerTenantId">If attempting to access a customer from a different tenancy,
        /// the tenant ID of that tenancy.</param>
        Task ThrowIfUserCannotModifyCustomer(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid customerId,
            Guid? customerTenantId = null);

        Task ThrowIfUserCannotModifyQuoteVersion(ClaimsPrincipal performingUser, Guid quoteVersionId);

        Task ThrowIfUserCannotModifyPolicyTransaction(ClaimsPrincipal performingUser, Guid policyTransactionId);

        Task ThrowIfUserCannotModifyClaimVersion(ClaimsPrincipal performingUser, Guid claimVersionId);

        Task ThrowIfUserCannotModifyTenant(Guid tenantId, ClaimsPrincipal performingUser);

        Task ThrowIfUserCannotModifyInvoice(ClaimsPrincipal performingUser, Guid invoiceId);

        Task ThrowIfUserCannotModifyBill(ClaimsPrincipal performingUser, Guid billId);

        Task ThrowIfUserCannotModifyCreditNote(ClaimsPrincipal performingUser, Guid creditNoteId);

        Task ThrowIfUserCannotModifyDebitNote(ClaimsPrincipal performingUser, Guid debitNoteId);

        Task ThrowIfUserCannotModifyCreditPayment(ClaimsPrincipal performingUser, Guid creditPaymentId);

        Task ThrowIfUserCannotModifyDebitPayment(ClaimsPrincipal performingUser, Guid debitPaymentId);

        Task ThrowIfUserCannotModifyProduct(ClaimsPrincipal performingUser, Guid productId);

        Task ThrowIfUserCannotModifyPortal(ClaimsPrincipal performingUser, Guid portalId);

        Task ThrowIfUserCannotViewQuoteVersion(ClaimsPrincipal performingUser, Guid? quoteVersionId);

        Task ThrowIfUserCannotViewPolicyTransaction(ClaimsPrincipal performingUser, Guid? policyTransactionId);

        Task ThrowIfUserCannotViewClaimVersion(ClaimsPrincipal performingUser, Guid? claimVersionId);

        Task ThrowIfUserCannotViewTenant(ClaimsPrincipal performingUser, Guid? tenantId);

        Task ThrowIfUserCannotViewInvoice(ClaimsPrincipal performingUser, Guid? invoiceId);

        Task ThrowIfUserCannotViewBill(ClaimsPrincipal performingUser, Guid? billId);

        Task ThrowIfUserCannotViewCreditNote(ClaimsPrincipal performingUser, Guid? creditNoteId);

        Task ThrowIfUserCannotViewDebitNote(ClaimsPrincipal performingUser, Guid? debitNoteId);

        Task ThrowIfUserCannotViewCreditPayment(ClaimsPrincipal performingUser, Guid? creditPaymentId);

        Task ThrowIfUserCannotViewDebitPayment(ClaimsPrincipal performingUser, Guid? debitPaymentId);

        Task ThrowIfUserCannotViewProduct(ClaimsPrincipal performingUser, Guid? productId);

        Task ThrowIfUserCannotViewPortal(ClaimsPrincipal performingUser, Guid? portalId);

        Task ThrowIfUserCannotViewReport(ClaimsPrincipal performingUser, Guid reportId);

        Task ThrowIfUserCannotModifyReport(ClaimsPrincipal performingUser, Guid reportId);

        Task ThrowIfUserCannotGenerateReport(ClaimsPrincipal performingUser, Guid reportId);

        /// <summary>
        /// Throws an exception only if the authenticated user cannot modify the policy or quote.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="policyId">The policy Id.</param>
        Task ThrowIfUserCannotModifyPolicyOrQuote(ClaimsPrincipal performingUser, Guid policyId);

        /// <summary>
        /// Throws an exception if the user can't access the data environment specified.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="environment">The data environment.</param>
        Task ThrowIfUserCannotAccessDataInEnvironment(
            ClaimsPrincipal performingUser, DeploymentEnvironment environment);

        /// <summary>
        /// Throws an exception if the user does not belong to master or the same specified tenant Id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="performingUser">The performing user.</param>
        void ThrowIfUserNotInTheSameOrMasterTenancy(Guid tenantId, ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if the user cannot update claim number.
        /// Only client users are able to assign/re-assign claim numbers.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        Task ThrowIfUserCannotUpdateClaimNumber(ClaimsPrincipal performingUser);

        /// <summary>
        /// Throws an exception if user cannot associate claim with policy.
        /// Only client users are able to associate claims with policy.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        Task ThrowIfUserCannotAssociateClaimWithPolicy(ClaimsPrincipal performingUser);

        void ThrowIfPortalIsDisabled(PortalReadModel portalDetails);

        /// <summary>
        /// Checks whether the things being queried are valid for the given user,
        /// and standardises the options to ensure they have the correct tenant id
        /// and organisation Id.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="options">The options.</param>
        /// <param name="allowDefaultOrgUsersToSeeOtherOrgData">
        ///         If true, default organisation users will be allowed to see other organisation data, so the
        ///      performing user's organisation ID will not be added to the options.
        /// </param>
        Task CheckAndStandardiseOptions(
            ClaimsPrincipal performingUser,
            IQueryOptionsModel options,
            bool allowDefaultOrgUsersToSeeOtherOrgData = false,
            bool restrictToOwnOrganisation = true);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        /// <param name="checkAdminUserPermission">Checks for admin user permissions.</param>
        Task ApplyRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters,
            bool checkAdminUserPermission = true);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view customer permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewCustomerRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view quote permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewQuoteRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view policy permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewPolicyRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view claim permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewClaimRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view message permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewMessageRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the modify user permission.
        /// </summary>
        /// <param name="performingUserTenantId">The performing users tenant Id.</param>
        /// <param name="performingUserId">The performing user Id.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyModifyUserRestrictionsToFilters(
           Guid performingUserTenantId,
           Guid performingUserId,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the modify user permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyModifyUserRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// Applies restrictions to the filters based upon the permissions of the performing user.
        /// Also applying more restrictions for the view role permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewRoleRestrictionsToFilters(
           ClaimsPrincipal performingUser,
           EntityListFilters filters);

        /// <summary>
        /// This is a temporary hardcode method for specific ride-protect organisation,
        /// to be removed after the implementation of UB-8372.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <param name="filters">The filters.</param>
        Task ApplyViewQuoteRestrictionsToFiltersForRideProtect(
            ClaimsPrincipal performingUser, EntityListFilters filters);

        /// <summary>
        /// Throw an exception error if not valid secret key or if not a master user.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        void ThrowIfNotValidSecretKeyAndNotMasterUser(ClaimsPrincipal performingUser);

        /// <summary>
        /// Determines whether the user has access to hangfire dashboard.
        /// </summary>
        /// <param name="performingUser">ClaimsPrincipal based on the recovered claims.</param>
        /// <returns>Returns true if the performing user has access to the dashboard.</returns>
        Task<bool> DoesUserHaveAccessToHangfireDashboard(ClaimsPrincipal performingUser);

        /// <summary>
        /// This will check and throw an exception if the user does not have permission to manage products.
        /// </summary>
        /// <param name="performingUser">ClaimsPrincipal object for the current performing user</param>
        /// <param name="action">The action being performed, used for exception message construction.</param>
        Task ThrowIfUserCannotManageOrganisationsAndProducts(ClaimsPrincipal performingUser, string action);

        /// <summary>
        /// Throws an exception if the user tries to delete customers without permission to import customers.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        Task ThrowIfUserDoesNotHaveImportCustomersPermission(ClaimsPrincipal performingUser);

        /// <summary>
        /// Determines whether the user has import claims permission.
        /// </summary>
        /// <param name="performingUser">The performing user.</param>
        /// <returns></returns>
        Task<bool> UserDoesHaveImportClaimsPermission(ClaimsPrincipal performingUser);
    }
}
