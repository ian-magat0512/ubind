// <copyright file="AuthorisationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Authorisation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Humanizer;
    using MoreLinq;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Helpers;
    using UBind.Application.Models.User;
    using UBind.Application.Policy;
    using UBind.Application.Policy.Transaction;
    using UBind.Application.Queries.Claim;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Queries.Principal;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Queries.Role;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Redis;
    using UBind.Domain.Services;

    /// <summary>
    /// Provides a number of common authorisation checks.
    /// </summary>
    public class AuthorisationService : IAuthorisationService
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IOrganisationService organisationService;
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly User.IUserService userService;
        private readonly ICustomerService customerService;
        private readonly IRoleService roleService;
        private readonly ICqrsMediator mediator;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IReportReadModelRepository reportReadModelRepository;
        private readonly IUserSessionService userSessionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorisationService"/> class.
        /// </summary>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="roleService">The role service.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="quoteAggregateResolverService">The quote aggregate resolver service.</param>
        /// <param name="dkimSettingRepository">The DKIM setting repository.</param>
        /// <param name="httpContextPropertiesResolver">The http context properties resolver.</param>
        /// <param name="reportReadModelRepository">The repository of the report.</param>
        public AuthorisationService(
            IOrganisationService organisationService,
            User.IUserService userService,
            ICustomerService customerService,
            IRoleService roleService,
            IPolicyReadModelRepository policyReadModelRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            ICqrsMediator mediator,
            ICachingResolver cachingResolver,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IDkimSettingRepository dkimSettingRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IReportReadModelRepository reportReadModelRepository,
            IUserSessionService userSessionService)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.cachingResolver = cachingResolver;
            this.organisationService = organisationService;
            this.userService = userService;
            this.customerService = customerService;
            this.roleService = roleService;
            this.mediator = mediator;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.dkimSettingRepository = dkimSettingRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.reportReadModelRepository = reportReadModelRepository;
            this.userSessionService = userSessionService;
        }

        public async Task<UserAuthorisationModel> GenerateUserAuthorisationModel(
            ClaimsPrincipal performingUser, Tenant tenant, string? serializedToken = null)
        {
            var userSessionModel = await this.userSessionService.Get(performingUser);
            if (userSessionModel == null)
            {
                throw new ArgumentException("An attempt was made to get the user session model for a user that does not have a session.");
            }

            var user = await this.mediator.Send(new GetUserByIdQuery(tenant.Id, userSessionModel.UserId));
            return await this.GenerateUserAuthorisationModel(user, tenant, serializedToken, userSessionModel);
        }

        public async Task<UserAuthorisationModel> GenerateUserAuthorisationModel(
            UserReadModel user, Tenant tenant, string? serializedToken = null, UserSessionModel? userSessionModel = null)
        {
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(user.TenantId, user.OrganisationId);

            // Get the default portal for the user. We'll use this if the portal user type doesn't match
            // (e.g. if it's a customer trying to login to an agent portal)
            var portalId = user.PortalId
                ?? await this.mediator.Send(new GetDefaultPortalIdQuery(
                    user.TenantId, user.OrganisationId, user.PortalUserType));
            string? portalOrganisationAlias = null;
            Guid? portalOrganisationId = null;
            if (portalId != null)
            {
                var portalModel = await this.cachingResolver.GetPortalOrThrow(tenant.Id, portalId.Value);
                portalOrganisationId = portalModel.OrganisationId;
                var portalOrganisation = await this.cachingResolver.GetOrganisationOrThrow(tenant.Id, portalOrganisationId.Value);
                portalOrganisationAlias = portalOrganisation.Alias;
            }
            List<Permission> permissions = await this.userService.GetEffectivePermissions(user, organisation);
            return new UserAuthorisationModel(
                user,
                serializedToken,
                tenant.Details.Alias,
                organisation.Alias,
                permissions,
                portalId,
                portalOrganisationId,
                portalOrganisationAlias,
                userSessionModel?.AuthenticationMethodId,
                userSessionModel?.AuthenticationMethodType,
                userSessionModel?.SamlSessionData?.SupportsSingleLogout);
        }

        /// <summary>
        /// Restricts data to a specific environment.
        /// </summary>
        public async Task ApplyEnvironmentRestrictionToFilters(ClaimsPrincipal performingUser, EntityListFilters filters)
        {
            var permissionEnvironmentMap = new Dictionary<DeploymentEnvironment, Permission>
            {
                { DeploymentEnvironment.Production, Permission.AccessProductionData },
                { DeploymentEnvironment.Staging, Permission.AccessStagingData },
                { DeploymentEnvironment.Development, Permission.AccessDevelopmentData },
            };

            if (filters.Environment != null)
            {
                var requiredPermission = permissionEnvironmentMap[filters.Environment.Value];
                if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, requiredPermission)))
                {
                    filters.Environment = null;
                }
            }

            if (filters.Environment == null)
            {
                if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Domain.Permissions.Permission.AccessProductionData)))
                {
                    filters.Environment = DeploymentEnvironment.Production;
                }
                else if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Domain.Permissions.Permission.AccessStagingData)))
                {
                    filters.Environment = DeploymentEnvironment.Staging;
                }
                else if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Domain.Permissions.Permission.AccessDevelopmentData)))
                {
                    filters.Environment = DeploymentEnvironment.Staging;
                }
                else
                {
                    filters.Environment = DeploymentEnvironment.None;
                }
            }
        }

        /// <summary>
        /// Restricts customers to seeing their own things only, and agents to seeing things they own.
        /// </summary>
        public async Task ApplyUserTypeRestrictionsToFilters(ClaimsPrincipal performingUser, EntityListFilters filters)
        {
            if (performingUser.GetUserType() == UserType.Customer)
            {
                // restrict customers to seeing their own things only
                filters.CustomerId = performingUser.GetCustomerId();
            }
            else
            {
                bool isTenantAdmin = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageTenantAdminUsers));
                bool isOrganisationAdmin = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageOrganisationAdminUsers));
                if (!isTenantAdmin && !isOrganisationAdmin)
                {
                    // restrict agents to seeing things they own
                    filters.OwnerUserId = performingUser.GetId();
                }
            }
        }

        /// <inheritdoc/>
        public void ThrowIfUserNotLoggedInAndTenantNotSpecified(ClaimsPrincipal performingUser, string tenant)
        {
            if ((performingUser == null || !performingUser.IsAuthenticated()) && string.IsNullOrEmpty(tenant))
            {
                throw new ErrorException(Errors.Account.NotLoggedInAndTenantNotSpecified());
            }
        }

        /// <inheritdoc/>
        public void ThrowIfUserNotInTenancy(Guid tenantId, ClaimsPrincipal performingUser)
        {
            if (performingUser.GetTenantId() != tenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized("access another tenant's data"));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserNotInOrganisationOrDefaultOrganisation(
            ClaimsPrincipal performingUser,
            Guid? organisationId,
            Guid? tenantId = null)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                authenticationData, organisationId, tenantId);
        }

        /// <inheritdoc/>
        public async Task ThrowIfOrganisationIsNotInTenancy(Guid tenantId, Guid organisationId)
        {
            var organisation = await this.organisationService
                .GetOrganisationSummaryForTenantIdAndOrganisationId(tenantId, organisationId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.Organisation.NotFound(organisationId));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotAccessTenant(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            string? action = null,
            string? entityName = null,
            dynamic? entityId = null)
        {
            IUserAuthenticationData userAuthenticationData
                = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            var userHasPermissionForTenants = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageTenants))
                || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewTenants));
            if (userAuthenticationData?.TenantId == Tenant.MasterTenantId && userHasPermissionForTenants)
            {
                // master users with manage tenant permission can see all tenants
                return;
            }
            if (tenantId == userAuthenticationData?.TenantId)
            {
                // users can see their own tenant
                return;
            }
            if (action != null && entityName != null && entityId != null)
            {
                throw new ErrorException(Errors.General.NotAuthorized(action, entityName, entityId));
            }
            if (action != null && entityName != null)
            {
                throw new ErrorException(Errors.General.NotAuthorized(action, entityName));
            }
            if (action != null)
            {
                throw new ErrorException(Errors.General.NotAuthorized(action));
            }
            throw new ErrorException(
                      Errors.General.Forbidden(reason: "your user account doesn't have access to the master tenancy"));
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserIsNotFromMasterTenant(
            ClaimsPrincipal performingUser,
            string? action = null)
        {
            IUserAuthenticationData userAuthenticationData
                = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            if (!performingUser.IsAuthenticated())
            {
                throw new ErrorException(Errors.General.NotAuthenticated(action));
            }

            var userHasPermissionForTenants = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageTenants))
                || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewTenants));
            if (userAuthenticationData?.TenantId == Tenant.MasterTenantId && userHasPermissionForTenants)
            {
                // master users with manage tenant permission can see all tenants
                return;
            }

            throw new ErrorException(
                Errors.General.Forbidden(action, "your user account doesn't have access to the master tenancy"));
        }

        /// <inheritdoc/>
        public bool IsUserFromMasterTenant(ClaimsPrincipal performingUser)
        {
            if (performingUser == null)
            {
                return false;
            }

            if (performingUser.IsAuthenticated() && performingUser.GetTenantIdOrNull() == Tenant.MasterTenantId)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotModifyClaim(
            ClaimsPrincipal performingUser,
            Guid claimId)
        {
            IClaimReadModelSummary? claim =
                this.claimReadModelRepository.GetSummaryById(performingUser.GetTenantId(), claimId);
            if (claim == null)
            {
                throw new ErrorException(Errors.Claim.NotFound(claimId));
            }

            await this.ThrowIfUserHasNoModifyClaimPermission(
                 claim.TenantId,
                 performingUser,
                 claim.OrganisationId,
                 claim.OwnerUserId,
                 claim.CustomerId,
                 claim.Id.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, claim.Environment);
        }

        public async Task ThrowIfRoleIsNotAssignableToUserUnderOrganisation(Guid tenantId, Guid roleId, Guid organisationId)
        {
            var role = this.roleService.GetRole(tenantId, roleId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
            var filter = new RoleReadModelFilters();
            filter.OrganisationIds = new Guid[] { organisationId };
            filter.TenantId = tenantId;
            var assignableQuery = new GetAssignableRolesMatchingFiltersQuery(filter);
            var assignableRoles = await this.mediator.Send(assignableQuery);
            if (!assignableRoles.Any(r => r.Id == roleId))
            {
                throw new ErrorException(Errors.Role.RoleIsNotAssignableToUserUnderOrganisation(role.Name, organisation.Name));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserIsAuthenticatedAndCannotModifyClaim(
            ClaimsPrincipal performingUser,
            Guid claimId)
        {
            if (!performingUser.IsAuthenticated())
            {
                return;
            }

            await this.ThrowIfUserCannotModifyClaim(performingUser, claimId);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotModifyCustomer(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid customerId,
            Guid? customerTenantId = null)
        {
            ICustomerReadModelSummary customer = this.customerService.GetCustomerById(tenantId, customerId);
            if (customer == null)
            {
                throw new ErrorException(Errors.Customer.NotFound(customerId));
            }

            await this.ThrowIfUserCannotViewCustomer(performingUser, customer);
            await this.ThrowIfUserHasNoModifyCustomerPermission(
                customer.TenantId,
                performingUser,
                customer.OrganisationId,
                customer.OwnerUserId,
                customer.Id,
                customer.Id.ToString());
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotModifyProduct(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid productId,
            string? action = null)
        {
            await this.ThrowIfUserCannotAccessTenant(tenantId, performingUser, action);
            var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
            if (product == null)
            {
                throw new ErrorException(Errors.Product.NotFound(productId.ToString()));
            }

            await this.ThrowIfUserCannotViewProduct(performingUser, product);

            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageProducts)))
            {
                throw new ErrorException(Errors.General.NotAuthorized(action != null ? action : "modify product."));
            }
        }

        public async Task ThrowIfUserCannotModifyQuoteVersion(ClaimsPrincipal performingUser, Guid quoteVersionId)
        {
            var quoteVersion = await this.mediator.Send(new GetQuoteVersionByIdQuery(
                performingUser.GetTenantId(), quoteVersionId));
            await this.ThrowIfUserCannotModifyQuote(performingUser, quoteVersion.QuoteId);
        }

        public async Task ThrowIfUserCannotModifyPolicyTransaction(ClaimsPrincipal performingUser, Guid policyTransactionId)
        {
            var policyTransaction = await this.mediator.Send(new GetPolicyTransactionByIdQuery(
                performingUser.GetTenantId(), policyTransactionId));
            await this.ThrowIfUserCannotModifyPolicy(performingUser, policyTransaction.PolicyId);
        }

        public async Task ThrowIfUserCannotModifyClaimVersion(ClaimsPrincipal performingUser, Guid claimVersionId)
        {
            var claimVersion = await this.mediator.Send(new GetClaimVersionByIdQuery(
                performingUser.GetTenantId(), claimVersionId));
            await this.ThrowIfUserCannotModifyClaim(performingUser, claimVersion.ClaimId);
        }

        public async Task ThrowIfUserCannotModifyTenant(Guid tenantId, ClaimsPrincipal performingUser)
        {
            await this.ThrowIfUserIsNotFromMasterTenant(performingUser);
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageTenants)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageTenants,
                    "tenant",
                    tenantId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyOrganisation(ClaimsPrincipal performingUser, Guid organisationId)
        {
            await this.ThrowIfOrganisationIsNotInTenancy(performingUser.GetTenantId(), organisationId);
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageOrganisations))
                && !await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageAllOrganisations)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageOrganisations,
                    "organisation",
                    organisationId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyInvoice(ClaimsPrincipal performingUser, Guid invoiceId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "invoice",
                    invoiceId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyBill(ClaimsPrincipal performingUser, Guid billId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "bill",
                    billId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyCreditNote(ClaimsPrincipal performingUser, Guid creditNoteId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "credit note",
                    creditNoteId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyDebitNote(ClaimsPrincipal performingUser, Guid debitNoteId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "debit note",
                    debitNoteId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyCreditPayment(ClaimsPrincipal performingUser, Guid creditPaymentId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "credit payment",
                    creditPaymentId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyDebitPayment(ClaimsPrincipal performingUser, Guid debitPaymentId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageCustomerAccounts,
                    "debit payment",
                    debitPaymentId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyProduct(ClaimsPrincipal performingUser, Guid productId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageProducts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManageProducts,
                    "product",
                    productId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotModifyPortal(ClaimsPrincipal performingUser, Guid portalId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManagePortals)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToModifyResource(
                    Permission.ManagePortals,
                    "portal",
                    portalId.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewQuoteVersion(ClaimsPrincipal performingUser, Guid? quoteVersionId)
        {
            QuoteVersionReadModel? quoteVersion = null;
            if (quoteVersionId.HasValue)
            {
                quoteVersion = await this.mediator.Send(new GetQuoteVersionByIdQuery(
                    performingUser.GetTenantId(), quoteVersionId.Value));
                EntityHelper.ThrowIfNotFound(quoteVersion, quoteVersionId.Value, "quote version");
            }

            await this.ThrowIfUserCannotViewQuote(performingUser, quoteVersion?.QuoteId);
        }

        public async Task ThrowIfUserCannotViewPolicyTransaction(ClaimsPrincipal performingUser, Guid? policyTransactionId)
        {
            PolicyTransaction? policyTransaction = null;
            if (policyTransactionId.HasValue)
            {
                policyTransaction = await this.mediator.Send(new GetPolicyTransactionByIdQuery(
                    performingUser.GetTenantId(), policyTransactionId.Value));
                EntityHelper.ThrowIfNotFound(policyTransaction, policyTransactionId.Value, "policy transaction");
            }

            await this.ThrowIfUserCannotViewPolicy(performingUser, policyTransaction?.PolicyId);
        }

        public async Task ThrowIfUserCannotViewClaimVersion(ClaimsPrincipal performingUser, Guid? claimVersionId)
        {
            ClaimVersionReadModel? claimVersion = null;
            if (claimVersionId.HasValue)
            {
                claimVersion = await this.mediator.Send(new GetClaimVersionByIdQuery(
                    performingUser.GetTenantId(), claimVersionId.Value));
                EntityHelper.ThrowIfNotFound(claimVersion, claimVersionId.Value, "claim version");
            }

            await this.ThrowIfUserCannotViewClaim(performingUser, claimVersion?.ClaimId);
        }

        public async Task ThrowIfUserCannotViewTenant(ClaimsPrincipal performingUser, Guid? tenantId)
        {
            await this.ThrowIfUserIsNotFromMasterTenant(performingUser);
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewTenants)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewTenants,
                    "tenant",
                    tenantId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewInvoice(ClaimsPrincipal performingUser, Guid? invoiceId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "invoice",
                    invoiceId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewBill(ClaimsPrincipal performingUser, Guid? billId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "bill",
                    billId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewCreditNote(ClaimsPrincipal performingUser, Guid? creditNoteId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "credit note",
                    creditNoteId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewDebitNote(ClaimsPrincipal performingUser, Guid? debitNoteId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "debit note",
                    debitNoteId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewCreditPayment(ClaimsPrincipal performingUser, Guid? creditPaymentId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "credit payment",
                    creditPaymentId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewDebitPayment(ClaimsPrincipal performingUser, Guid? debitPaymentId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomerAccounts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewCustomerAccounts,
                    "debit payment",
                    debitPaymentId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewProduct(ClaimsPrincipal performingUser, Guid? productId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewProducts)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewProducts,
                    "product",
                    productId?.ToString()));
            }
        }

        public async Task ThrowIfUserCannotViewReport(ClaimsPrincipal performingUser, Guid reportId)
        {
            ReportReadModel? reportDetails = this.reportReadModelRepository.SingleOrDefaultIncludeAllProperties(performingUser.GetTenantId(), reportId);
            reportDetails = EntityHelper.ThrowIfNotFound(reportDetails, reportId, "report");
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewReports)) || reportDetails.OrganisationId != performingUser.GetOrganisationId())
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }

        public async Task ThrowIfUserCannotModifyReport(ClaimsPrincipal performingUser, Guid reportId)
        {
            ReportReadModel? reportDetails = this.reportReadModelRepository.SingleOrDefaultIncludeAllProperties(performingUser.GetTenantId(), reportId);
            reportDetails = EntityHelper.ThrowIfNotFound(reportDetails, reportId, "report");
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageReports)) || reportDetails.OrganisationId != performingUser.GetOrganisationId())
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }

        public async Task ThrowIfUserCannotGenerateReport(ClaimsPrincipal performingUser, Guid reportId)
        {
            ReportReadModel? reportDetails = this.reportReadModelRepository.SingleOrDefaultIncludeAllProperties(performingUser.GetTenantId(), reportId);
            reportDetails = EntityHelper.ThrowIfNotFound(reportDetails, reportId, "report");
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.GenerateReports)) || reportDetails.OrganisationId != performingUser.GetOrganisationId())
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }

        public async Task ThrowIfUserCannotViewPortal(ClaimsPrincipal performingUser, Guid? portalId)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewPortals)))
            {
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewPortals,
                    "portal",
                    portalId?.ToString()));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewQuote(
            ClaimsPrincipal performingUser,
            Guid? quoteId = null,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            NewQuoteReadModel? quote = null;
            if (quoteId.HasValue)
            {
                quote = this.quoteReadModelRepository.GetById(tenantId.Value, quoteId.Value);
                if (quote == null)
                {
                    throw new ErrorException(Errors.Quote.NotFound(quoteId.Value));
                }

                await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, quote.Environment);
            }

            await this.ThrowIfUserHasNoViewQuotePermission(
                quote?.TenantId ?? performingUser.GetTenantId(),
                performingUser,
                quote?.OrganisationId,
                quote?.OwnerUserId,
                quote?.CustomerId,
                quote?.ProductId,
                quote?.Id.ToString());
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewQuoteVersion(ClaimsPrincipal performingUser, Guid quoteVersionId, Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            var quoteVersion = await this.mediator.Send(new GetQuoteVersionByIdQuery(tenantId.Value, quoteVersionId));
            EntityHelper.ThrowIfNotFound(quoteVersion, quoteVersionId);

            await this.ThrowIfUserHasNoViewQuotePermission(
                quoteVersion.TenantId,
                performingUser,
                quoteVersion.OrganisationId,
                quoteVersion.OwnerUserId,
                quoteVersion.CustomerId,
                quoteVersion.ProductId,
                quoteVersion.QuoteId.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, quoteVersion.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotModifyQuote(
            ClaimsPrincipal performingUser,
            Guid quoteId,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            NewQuoteReadModel quote = this.quoteReadModelRepository.GetById(tenantId.Value, quoteId);
            if (quote == null)
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId));
            }

            await this.ThrowIfUserHasNoModifyQuotePermission(
                quote.TenantId,
                performingUser,
                quote.OrganisationId,
                quote.OwnerUserId,
                quote.CustomerId,
                quote.Id.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, quote.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotModifyPolicy(
            ClaimsPrincipal performingUser,
            Guid policyId)
        {
            PolicyReadModel policy = this.policyReadModelRepository.GetById(performingUser.GetTenantId(), policyId);
            if (policy == null)
            {
                throw new ErrorException(Errors.Policy.NotFound(policyId));
            }

            await this.ThrowIfUserHasNoModifyPolicyPermission(
                policy.TenantId,
                performingUser,
                policy.OrganisationId,
                policy.OwnerUserId,
                policy.CustomerId,
                policy.Id.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, policy.Environment);
        }

        public async Task ThrowIfUserCannotModifyPolicyOrQuote(
            ClaimsPrincipal performingUser,
            Guid policyId)
        {
            var environment = DeploymentEnvironment.None;
            var policy = this.policyReadModelRepository.GetById(performingUser.GetTenantId(), policyId);
            if (policy != null)
            {
                environment = policy.Environment;
                await this.ThrowIfUserHasNoModifyPolicyPermission(
                    policy.TenantId,
                    performingUser,
                    policy.OrganisationId,
                    policy.OwnerUserId,
                    policy.CustomerId,
                    policy.Id.ToString());
            }
            else
            {
                var quote = this.quoteAggregateResolverService.GetQuoteAggregateForPolicy(performingUser.GetTenantId(), policyId);
                if (quote != null)
                {
                    environment = quote.Environment;
                    await this.ThrowIfUserHasNoModifyQuotePermission(
                        quote.TenantId,
                        performingUser,
                        quote.OrganisationId,
                        quote.OwnerUserId,
                        quote.CustomerId,
                        quote.Id.ToString());
                }
            }

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewQuote(ClaimsPrincipal performingUser, IQuoteReadModelSummary quote)
        {
            await this.ThrowIfUserHasNoViewQuotePermission(
                quote.TenantId,
                performingUser,
                quote.OrganisationId,
                quote.OwnerUserId,
                quote.CustomerId,
                quote.ProductId,
                quote.QuoteId.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, quote.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewClaim(ClaimsPrincipal performingUser, Guid? claimId)
        {
            IClaimReadModelSummary? claim = null;
            if (claimId.HasValue)
            {
                claim = await this.mediator.Send(new GetClaimSummaryByIdQuery(performingUser.GetTenantId(), claimId.Value));
                EntityHelper.ThrowIfNotFound(claim, claimId.Value);
            }

            await this.ThrowIfUserCannotViewClaim(performingUser, claim);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewClaimVersion(ClaimsPrincipal performingUser, Guid claimVersionId)
        {
            var claimVersion = await this.mediator.Send(new GetClaimVersionByIdQuery(performingUser.GetTenantId(), claimVersionId));
            EntityHelper.ThrowIfNotFound(claimVersion, claimVersionId);
            await this.ThrowIfUserHasNoViewClaimPermission(
                claimVersion.TenantId,
                performingUser,
                claimVersion.OrganisationId,
                claimVersion.OwnerUserId,
                claimVersion.CustomerId,
                claimVersion.ProductId,
                claimVersion.PolicyId.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, claimVersion.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewClaim(ClaimsPrincipal performingUser, IClaimReadModelSummary? claim)
        {
            await this.ThrowIfUserHasNoViewClaimPermission(
                claim?.TenantId ?? performingUser.GetTenantId(),
                performingUser,
                claim?.OrganisationId ?? performingUser.GetOrganisationId(),
                claim?.OwnerUserId,
                claim?.CustomerId,
                claim?.ProductId,
                claim?.PolicyId.ToString());

            if (claim != null)
            {
                await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, claim.Environment);
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserIsAuthenticatedAndCannotViewClaim(ClaimsPrincipal performingUser, Guid claimId)
        {
            if (!performingUser.IsAuthenticated())
            {
                return;
            }

            IClaimReadModelSummary? claim =
                this.claimReadModelRepository.GetSummaryById(performingUser.GetTenantId(), claimId);
            if (claim == null)
            {
                throw new ErrorException(Errors.Claim.NotFound(claimId));
            }

            await this.ThrowIfUserCannotViewClaim(performingUser, claim);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewPolicy(
            ClaimsPrincipal performingUser,
            Guid policyId,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            var policy = this.policyReadModelRepository.GetById(tenantId.Value, policyId);
            EntityHelper.ThrowIfNotFound(policy, policyId, "policy");
            await this.ThrowIfUserHasNoViewPolicyPermission(
                policy.TenantId,
                performingUser,
                policy.OrganisationId,
                policy.OwnerUserId,
                policy.CustomerId,
                policy.ProductId,
                policy.Id.ToString());

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, policy.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, IPolicyReadModelSummary policy)
        {
            await this.ThrowIfUserHasNoViewPolicyPermission(
                policy?.TenantId ?? performingUser.GetTenantId(),
                performingUser,
                policy?.OrganisationId,
                policy?.OwnerUserId,
                policy?.CustomerId,
                policy?.ProductId,
                policy?.PolicyId.ToString());

            if (policy != null)
            {
                await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, policy.Environment);
            }
        }

        public async Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, Guid? policyId)
        {
            PolicyReadModel? policy = null;
            if (policyId.HasValue)
            {
                var userTenantId = performingUser.GetTenantId();
                policy = await this.mediator.Send(new GetPolicyByIdQuery(userTenantId, policyId.Value));
                EntityHelper.ThrowIfNotFound(policy, policyId.Value, "policy");
            }

            await this.ThrowIfUserCannotViewPolicy(performingUser, policy);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewPolicy(ClaimsPrincipal performingUser, PolicyReadModel? policy)
        {
            await this.ThrowIfUserHasNoViewPolicyPermission(
                policy?.TenantId ?? performingUser.GetTenantId(),
                performingUser,
                policy?.OrganisationId,
                policy?.OwnerUserId,
                policy?.CustomerId,
                policy?.ProductId,
                policy?.Id.ToString());

            if (policy != null)
            {
                await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, policy.Environment);
            }
        }

        public async Task ThrowIfUserCannotViewCustomerWithViewCustomerOrViewUserPermission(
            ClaimsPrincipal performingUser, ICustomerReadModelSummary customer)
        {
            if (await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewCustomers))
                || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewAllCustomers)))
            {
                await this.ThrowIfUserHasNoViewCustomerPermission(
                    customer.TenantId,
                    performingUser,
                    customer.OrganisationId,
                    customer.OwnerUserId,
                    customer.Id,
                    customer.Id.ToString());
            }
            else
            {
                await this.ThrowIfUserHasNoViewUserPermission(
                    customer.TenantId,
                    performingUser,
                    customer.OrganisationId,
                    customer.Id.ToString());
            }

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, customer.Environment);
        }

        public async Task ThrowIfUserCannotViewCustomer(ClaimsPrincipal performingUser, Guid? customerId)
        {
            if (customerId.HasValue)
            {
                var customer = await this.mediator.Send(
                    new GetCustomerByIdQuery(performingUser.GetTenantId(), customerId.Value));
                await this.ThrowIfUserCannotViewCustomer(performingUser, customer);
            }
            else
            {
                await this.ThrowIfUserHasNoPermission(
                    performingUser.GetTenantId(),
                    performingUser,
                    performingUser.GetOrganisationId(),
                    null,
                    null,
                    "customer",
                    null,
                    Permission.ViewCustomers,
                    Permission.ViewAllCustomers,
                    null);
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewCustomer(
            ClaimsPrincipal performingUser, ICustomerReadModelSummary customer)
        {
            await this.ThrowIfUserHasNoViewCustomerPermission(
                customer.TenantId,
                performingUser,
                customer.OrganisationId,
                customer.OwnerUserId,
                customer.Id,
                customer.Id.ToString());

            bool hasRequiredCustomerPermission
                = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageMasterAdminUsers))
                    || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageOrganisationAdminUsers))
                    || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ManageAllCustomers))
                    || await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewAllCustomers));
            if (!hasRequiredCustomerPermission && performingUser.GetId() != customer.OwnerUserId)
            {
                // you can't see customers you don't own, sorry
                throw new ErrorException(Errors.General.AccessDeniedToResource("customer", customer.Id.ToString()));
            }

            await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, customer.Environment);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewProduct(
            ClaimsPrincipal performingUser, Domain.Product.Product product, string? action = null)
        {
            string entityName = "product";
            await this.ThrowIfUserCannotAccessTenant(
                product.TenantId, performingUser, action, entityName, product.Details.Name);
            bool hasPermission
                = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ViewProducts));
            if (!hasPermission)
            {
                // you don't have the basic permission to view product, sorry.
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewProducts,
                    entityName,
                    product.Id.ToString()));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotViewEmail(ClaimsPrincipal performingUser, IEmailDetails email)
        {
            if (email.User != null && email.User.Id == performingUser.GetId())
            {
                // users can always see their own user account emails
                return;
            }

            Guid? ownerUserId = null;
            Guid? customerId = email.Customer?.Id;
            if (customerId != null)
            {
                var customerRecord = this.customerService.GetCustomerById(email.TenantId, customerId.Value);
                ownerUserId = customerRecord.OwnerUserId;
            }
            else if (email.User != null)
            {
                ownerUserId = performingUser.GetId();
            }

            // get customer id from relationships if there has.
            if (email.Customer == null)
            {
                var customerRecipentRelationship = email.Relationships
                    .FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient
                        && x.ToEntityType == EntityType.Customer);
                var customerRelationship = email.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
                customerId = customerRecipentRelationship?.ToEntityId ?? customerRelationship?.FromEntityId;
            }

            if (ownerUserId == null)
            {
                var toOwner = email.Relationships.FirstOrDefault(y => y.ToEntityType == EntityType.User);
                var fromOwner = email.Relationships.FirstOrDefault(y => y.FromEntityType == EntityType.User);

                // if the email has no direct relationship (messageSender/messageReceipient) to the user,
                // and if the email is a quote email, then use the OwnerUserId from the quote
                ownerUserId = toOwner?.Id ?? fromOwner?.Id ?? email.Quote?.OwnerUserId;
            }

            await this.ThrowIfUserHasNoViewEmailPermission(
               email.TenantId,
               performingUser,
               email.OrganisationId,
               ownerUserId,
               customerId,
               email.Id.ToString());

            if (performingUser.GetTenantId() == Tenant.MasterTenantId
                && email.TenantId == Tenant.MasterTenantId)
            {
                // email and user is from master.
                return;
            }

            string? environment = email.Tags.FirstOrDefault(x => x.TagType == TagType.Environment)?.Value;
            if (!string.IsNullOrEmpty(environment))
            {
                Enum.TryParse(environment, out DeploymentEnvironment deploymentEnvironment);
                await this.ThrowIfUserCannotAccessDataInEnvironment(performingUser, deploymentEnvironment);
            }
        }

        public void ThrowIfPortalIsDisabled(PortalReadModel portal)
        {
            if (portal.Disabled)
            {
                string action = "access this portal";
                string reason = "it has been disabled";
                throw new ErrorException(Errors.General.Forbidden(action, reason));
            }
        }

        /// <inheritdoc/>
        public async Task CheckAndStandardiseOptions(
            ClaimsPrincipal performingUser,
            IQueryOptionsModel options,
            bool allowDefaultOrgUsersToSeeOtherOrgData = false,
            bool restrictToOwnOrganisation = true)
        {
            options.Environment = string.IsNullOrEmpty(options.Environment) ? "production" : options.Environment;
            EnvironmentHelper.ParseEnvironmentOrThrow(options.Environment);
            PagingHelper.ThrowIfPageNumberInvalid(options.Page);
            options.Tenant = options.Tenant ?? performingUser.GetTenantId().ToString();
            if (performingUser.IsMasterUser())
            {
                // master users queries are not restricted to the default organisation.
                return;
            }

            string? specifiedOrganisationOrNone = options.Organisation;

            if (restrictToOwnOrganisation)
            {
                // restrict by default
                options.Organisation = performingUser.GetOrganisationId().ToString();

                // now let's see if we can be less restrictive
                if (allowDefaultOrgUsersToSeeOtherOrgData)
                {
                    var tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(options.Tenant));
                    var performingUserOrganisation = await this.cachingResolver.GetOrganisationOrNull(new GuidOrAlias(tenant?.Id), new GuidOrAlias(options.Organisation));
                    var isPerformingUserFromDefaultOrganisation = performingUserOrganisation != null && await this.organisationService
                        .IsOrganisationDefaultForTenant(performingUserOrganisation.TenantId, performingUserOrganisation.Id);
                    if (isPerformingUserFromDefaultOrganisation)
                    {
                        options.Organisation = specifiedOrganisationOrNone;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task ApplyModifyUserRestrictionsToFilters(
            Guid performingUserTenantId,
            Guid performingUserId,
            EntityListFilters filters)
        {
            IUserReadModelSummary performingUser = this.userService.GetUser(performingUserTenantId, performingUserId);
            var authenticationData = new UserAuthenticationData(performingUser);
            await this.ApplyRestrictionsToFilters(
               authenticationData,
               filters,
               "user",
               null,
               Permission.ManageUsers,
               Permission.ManageUsersForOtherOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyModifyUserRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ApplyRestrictionsToFilters(
               authenticationData,
               filters,
               "user",
               null,
               Permission.ManageUsers,
               Permission.ManageUsersForOtherOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewRoleRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            await this.ApplyRestrictionsToFilters(
               await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser)),
               filters,
               "role",
               null,
               Permission.ViewRoles,
               Permission.ViewRolesFromAllOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewCustomerRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            await this.ApplyRestrictionsToFilters(
               await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser)),
               filters,
               "customer",
               Permission.ViewCustomers,
               Permission.ViewAllCustomers,
               Permission.ViewAllCustomersFromAllOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewQuoteRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            await this.ApplyRestrictionsToFilters(
               await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser)),
               filters,
               "quote",
               Permission.ViewQuotes,
               Permission.ViewAllQuotes,
               Permission.ViewAllQuotesFromAllOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewQuoteRestrictionsToFiltersForRideProtect(
           ClaimsPrincipal performingUser,
           EntityListFilters filters)
        {
            bool isPerformingUserFromRideProtectOrganisation =
                await this.IsPerformingUserFromRideProtectOrganisation(performingUser.GetTenantId(), performingUser.GetOrganisationId());
            if (isPerformingUserFromRideProtectOrganisation)
            {
                await this.UpdateFilterConditionForRideProtectOrganisation(performingUser.GetTenantId(), filters);
            }
        }

        /// <inheritdoc/>
        public async Task ApplyViewPolicyRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ApplyRestrictionsToFilters(
               authenticationData,
               filters,
               "policy",
               Permission.ViewPolicies,
               Permission.ViewAllPolicies,
               Permission.ViewAllPoliciesFromAllOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewClaimRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ApplyRestrictionsToFilters(
                 authenticationData,
                 filters,
                 "claim",
                 Permission.ViewClaims,
                 Permission.ViewAllClaims,
                 Permission.ViewAllClaimsFromAllOrganisations);
        }

        /// <inheritdoc/>
        public async Task ApplyViewMessageRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters)
        {
            filters.Environment = performingUser.IsMasterUser() ? null : filters.Environment;

            var tenantRestrictedPermission =
                !filters.Statuses.Contains("admin") && !filters.Statuses.Contains("users")
                    ? Permission.ViewAllCustomersFromAllOrganisations
                    : (Permission?)null;

            var userAuthenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));

            if (tenantRestrictedPermission != null
                && filters.EntityType != null
                && userAuthenticationData.HasPermission(tenantRestrictedPermission.Value))
            {
                filters.CanViewEmailsFromOtherOrgThatHasCustomer = true;
                filters.PerformingUserOrganisationId = performingUser.GetOrganisationId();
            }

            await this.ApplyRestrictionsToFilters(
                userAuthenticationData,
                filters,
                "message",
                Permission.ViewMessages,
                Permission.ViewAllMessages,
                tenantRestrictedPermission);
        }

        /// <inheritdoc/>
        public async Task ApplyRestrictionsToFilters(
            ClaimsPrincipal performingUser,
            EntityListFilters filters,
            bool checkAdminUserPermission = true)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ApplyRestrictionsToFilters(authenticationData, filters, checkAdminUserPermission);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotAccessDataInEnvironment(
            ClaimsPrincipal performingUser,
            DeploymentEnvironment environment)
        {
            var authenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            await this.ThrowIfUserCannotAccessDataInEnvironment(authenticationData, environment);
        }

        /// <inheritdoc/>
        public void ThrowIfUserNotInTheSameOrMasterTenancy(Guid tenantId, ClaimsPrincipal performingUser)
        {
            var userTenantId = performingUser.GetTenantId();
            if (tenantId != userTenantId && userTenantId != Tenant.MasterTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden("get users from another tenancy"));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotUpdateClaimNumber(ClaimsPrincipal performingUser)
        {
            string action = "update claim number";
            this.ThrowIfUserIsNotAgent(await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser)), action);
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserCannotAssociateClaimWithPolicy(ClaimsPrincipal performingUser)
        {
            string action = "associate claim with another policy";
            this.ThrowIfUserIsNotAgent(await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser)), action);
        }

        /// <inheritdoc/>
        public void ThrowIfNotValidSecretKeyAndNotMasterUser(ClaimsPrincipal performingUser)
        {
            if (performingUser.GetUserType() != UserType.Master && !this.httpContextPropertiesResolver.IsValidSecretKey())
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DoesUserHaveAccessToHangfireDashboard(ClaimsPrincipal performingUser)
        {
            IUserAuthenticationData userAuthenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            bool hasAccessToHangfire = userAuthenticationData.UserType == UserType.Master && userAuthenticationData.HasPermission(Permission.ManageBackgroundJobs);
            return hasAccessToHangfire;
        }

        public Task ThrowIfUserCannotAccessDataInEnvironment(
            IUserAuthenticationData userAuthenticationData,
            DeploymentEnvironment environment)
        {
            if (environment == DeploymentEnvironment.None)
            {
                return Task.CompletedTask;
            }

            var permissionEnvironmentMap = new Dictionary<DeploymentEnvironment, Permission>
            {
                { DeploymentEnvironment.Production, Permission.AccessProductionData },
                { DeploymentEnvironment.Staging, Permission.AccessStagingData },
                { DeploymentEnvironment.Development, Permission.AccessDevelopmentData },
            };
            permissionEnvironmentMap.TryGetValue(environment, out Permission permission);
            if (!userAuthenticationData.HasPermission(permission))
            {
                throw new ErrorException(Domain.Errors.General.AccessDeniedToEnvironment(environment));
            }

            return Task.CompletedTask;
        }

        public async Task ThrowIfUserCannotManageOrganisationsAndProducts(ClaimsPrincipal performingUser, string action)
        {
            IUserAuthenticationData userAuthenticationData = await this.mediator.Send(new GetPrincipalAuthenticationDataQuery(performingUser));
            if (!(userAuthenticationData.HasPermission(Permission.ManageOrganisations)
                 || userAuthenticationData.HasPermission(Permission.ManageAllOrganisations))
                && !userAuthenticationData.HasPermission(Permission.ManageProducts))
            {
                // This is added as it is raised on R12 defects, this solution is based on the A/C documents on the provided link below:
                // https://confluence.aptiture.com/pages/viewpage.action?pageId=98476434
                throw new ErrorException(Errors.General.NotAuthorized(action));
            }
        }

        /// <inheritdoc/>
        public async Task ThrowIfUserDoesNotHaveImportCustomersPermission(ClaimsPrincipal performingUser)
        {
            if (!await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ImportCustomers)))
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }

        /// <inheritdoc/>
        public async Task<bool> UserDoesHaveImportClaimsPermission(ClaimsPrincipal performingUser)
        {
            return await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, Permission.ImportClaims));
        }

        private async Task ThrowIfUserNotInOrganisationOrDefaultOrganisation(
            IUserAuthenticationData userAuthenticationData,
            Guid? organisationId,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? userAuthenticationData.TenantId;
            this.ThrowIfUserNotInTenancy(tenantId.Value, userAuthenticationData);
            if (organisationId.HasValue && userAuthenticationData.OrganisationId != organisationId)
            {
                var fromDefaultOrganisation = await this.organisationService.IsOrganisationDefaultForTenant(
                    tenantId.Value, userAuthenticationData.OrganisationId);
                if (!fromDefaultOrganisation)
                {
                    throw new ErrorException(Errors.General.NotAuthorized("access another organisation's data"));
                }
            }
        }

        private void ThrowIfUserNotInTenancy(Guid tenantId, IUserAuthenticationData performingUser)
        {
            if (performingUser.TenantId != tenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized("access another tenant's data"));
            }
        }

        private async Task ApplyRestrictionsToFilters(
            IUserAuthenticationData userAuthenticationData,
            EntityListFilters filters,
            bool checkAdminUserPermission = true)
        {
            //TODO: this will need to be revisted as all authorisation checks need refactoring
            Guid? firstOrganisationid = filters.OrganisationIds?.FirstOrDefault();
            var organisation = await this.cachingResolver.GetOrganisationOrNull(
                new GuidOrAlias(filters.TenantId ?? Guid.Empty), new GuidOrAlias(firstOrganisationid));
            await this.ThrowIfUserNotInOrganisationOrDefaultOrganisation(
                userAuthenticationData,
                organisation?.Id,
                filters.TenantId);

            if (filters.Environment != null)
            {
                await this.ThrowIfUserCannotAccessDataInEnvironment(
                    userAuthenticationData,
                    filters.Environment.Value);
            }

            if (checkAdminUserPermission)
            {
                filters.OrganisationIds = new Guid[] { userAuthenticationData.OrganisationId };
                if (userAuthenticationData.UserType == UserType.Customer)
                {
                    // restrict customers to seeing their own things only
                    filters.CustomerId = userAuthenticationData.CustomerId;
                }
                else
                {
                    bool isTenantAdmin = userAuthenticationData.HasPermission(Permission.ManageTenantAdminUsers);
                    bool isOrganisationAdmin = userAuthenticationData.HasPermission(Permission.ManageOrganisationAdminUsers);
                    if (!isTenantAdmin && !isOrganisationAdmin)
                    {
                        // restrict agents to seeing things they own
                        filters.OwnerUserId = userAuthenticationData.UserId;
                    }
                }
            }
        }

        private async Task ThrowIfUserCannotModifyRole(
            ClaimsPrincipal performingUser,
            Guid? organisationId = null,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            Guid targetOrganisationId
                = organisationId == null ? performingUser.GetOrganisationId() : organisationId.Value;

            await this.ThrowIfOrganisationIsNotInTenancy(tenantId.Value, targetOrganisationId);

            await this.ThrowIfUserHasNoModifyRolePermission(
                tenantId.Value,
                performingUser,
                targetOrganisationId);
        }

        private async Task ThrowIfUserCannotModifyUsers(
            ClaimsPrincipal performingUser,
            Guid? organisationId = null,
            Guid? tenantId = null)
        {
            tenantId = tenantId ?? performingUser.GetTenantId();
            Guid targetOrganisationId = organisationId ?? performingUser.GetOrganisationId();

            await this.ThrowIfOrganisationIsNotInTenancy(tenantId.Value, targetOrganisationId);

            await this.ThrowIfUserHasNoModifyUserPermission(
                tenantId.Value,
                performingUser,
                targetOrganisationId,
                null);
        }

        /// <summary>
        /// Apply filters based on permissions.
        /// </summary>
        /// <param name="userAuthenticationData">The performing user.</param>
        /// <param name="filters">The filters to apply the modification on.</param>
        /// <param name="resourceName">The resource name ex. "claim" or "quote". Will be used in error message.</param>
        /// <param name="ownershipRestrictedPermission">The permission type that is restricted by ownership of the record.
        /// example: ViewClaims.</param>
        /// <param name="organisationRestrictedPermission">The permission type that is restricted by organisation and tenant of the record.
        /// example: ViewAllClaims.</param>
        /// <param name="tenantRestrictedPermission">The permission type that is restricted by tenant only of the record.
        /// example: ViewAllClaimsFromAllOrganisations.</param>
        private async Task ApplyRestrictionsToFilters(
            IUserAuthenticationData userAuthenticationData,
            EntityListFilters filters,
            string resourceName,
            Permission? ownershipRestrictedPermission,
            Permission? organisationRestrictedPermission,
            Permission? tenantRestrictedPermission)
        {
            if (userAuthenticationData.TenantId == Tenant.MasterTenantId
                && (userAuthenticationData.HasPermission(Permission.ViewUsersFromOtherOrganisations)
                    || userAuthenticationData.HasPermission(Permission.ManageUsersForOtherOrganisations)))
            {
                // this master tenant user can view all user records
                return;
            }

            var isPerformingUserFromDefaultOrganisation =
                await this.organisationService.IsOrganisationDefaultForTenant(
                    userAuthenticationData.TenantId, userAuthenticationData.OrganisationId);

            // can access all records within a tenant, most probably this was set already,
            // but to make sure we set it again.
            filters.TenantId = filters.TenantId ?? userAuthenticationData.TenantId;
            if (tenantRestrictedPermission != null && userAuthenticationData.HasPermission(tenantRestrictedPermission.Value)
                && isPerformingUserFromDefaultOrganisation)
            {
                // can access all within tenant.
                // force empty to not filter with organisationId.
                filters.OrganisationIds = Enumerable.Empty<Guid>();
            }
            else if (organisationRestrictedPermission != null
                && userAuthenticationData.HasPermission(organisationRestrictedPermission.Value))
            {
                // can only access records from their own organisation.
                filters.OrganisationIds = new Guid[] { userAuthenticationData.OrganisationId };
            }
            else if (ownershipRestrictedPermission != null
                && userAuthenticationData.HasPermission(ownershipRestrictedPermission.Value))
            {
                if (userAuthenticationData.UserType == UserType.Customer)
                {
                    // customer can only access their own records.
                    filters.CustomerId = userAuthenticationData.CustomerId;
                }
                else
                {
                    // clients can see records they are owner of.
                    filters.OwnerUserId = userAuthenticationData.UserId;
                }
            }
            else if (ownershipRestrictedPermission != null)
            {
                // you don't have the basic permission to view, sorry.
                throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    (Permission)ownershipRestrictedPermission,
                    resourceName,
                    null));
            }

            await this.ApplyRestrictionsToFilters(userAuthenticationData, filters, false);
        }

        private async Task ThrowIfUserHasNoModifyRolePermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                null,
                null,
                "role",
                null,
                null,
                Permission.ManageRoles,
                Permission.ManageRolesForAllOrganisations);
        }

        private async Task ThrowIfUserHasNoModifyUserPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            string? resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                null,
                null,
                "user",
                resourceId,
                null,
                Permission.ManageUsers,
                Permission.ManageUsersForOtherOrganisations);
        }

        private async Task ThrowIfUserHasNoViewEmailPermission(
          Guid tenantId,
          ClaimsPrincipal performingUser,
          Guid organisationId,
          Guid? ownerId,
          Guid? recipientCustomerId,
          string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                recipientCustomerId,
                "email",
                resourceId,
                Permission.ViewMessages,
                Permission.ViewAllMessages,
                Permission.ViewAllMessages);
        }

        private async Task ThrowIfUserHasNoModifyClaimPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid? customerId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "claim",
                resourceId,
                Permission.ManageClaims,
                Permission.ManageAllClaims,
                Permission.ManageAllClaimsForAllOrganisations);
        }

        private async Task ThrowIfUserHasNoModifyQuotePermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid? customerId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "quote",
                resourceId,
                Permission.ManageQuotes,
                Permission.ManageAllQuotes,
                Permission.ManageAllQuotesForAllOrganisations);
        }

        private async Task ThrowIfUserHasNoModifyPolicyPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid? customerId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "policy",
                resourceId,
                Permission.ManagePolicies,
                Permission.ManageAllPolicies,
                Permission.ManageAllPoliciesForAllOrganisations);
        }

        private async Task ThrowIfUserHasNoModifyCustomerPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid? customerId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "customer",
                resourceId,
                Permission.ManageCustomers,
                Permission.ManageAllCustomers,
                Permission.ManageAllCustomersForAllOrganisations);
        }

        private async Task ThrowIfUserHasNoViewRolePermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                null,
                null,
                "role",
                resourceId,
                null,
                Permission.ViewRoles,
                Permission.ViewRolesFromAllOrganisations);
        }

        private async Task ThrowIfUserHasNoViewUserPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                null,
                null,
                "user",
                resourceId,
                null,
                Permission.ViewUsers,
                Permission.ViewUsersFromOtherOrganisations);
        }

        private async Task ThrowIfUserHasNoViewClaimPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid? customerId,
            Guid? productId,
            string? resourceId)
        {
            bool productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation =
                await this.ProductAndOrganisationIsRideProtectOrPerformingUserFromDefaultOrganisation(performingUser, tenantId, productId);

            var tenantRestrictedPermission = productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation
                ? Permission.ViewAllClaimsFromAllOrganisations : (Permission?)null;

            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "claim",
                resourceId,
                Permission.ViewClaims,
                Permission.ViewAllClaims,
                tenantRestrictedPermission);
        }

        private async Task ThrowIfUserHasNoViewPolicyPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid? policyOrganisationId,
            Guid? ownerId,
            Guid? customerId,
            Guid? productId,
            string? resourceId)
        {
            bool productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation =
                await this.ProductAndOrganisationIsRideProtectOrPerformingUserFromDefaultOrganisation(performingUser, tenantId, productId);

            var tenantRestrictedPermission = productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation
                ? Permission.ViewAllPoliciesFromAllOrganisations : (Permission?)null;

            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                policyOrganisationId,
                ownerId,
                customerId,
                "policy",
                resourceId,
                Permission.ViewPolicies,
                Permission.ViewAllPolicies,
                tenantRestrictedPermission);
        }

        private async Task ThrowIfUserHasNoViewQuotePermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid? quoteOrganisationId,
            Guid? ownerId,
            Guid? customerId,
            Guid? productId,
            string? resourceId)
        {
            bool productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation =
                 await this.ProductAndOrganisationIsRideProtectOrPerformingUserFromDefaultOrganisation(performingUser, tenantId, productId);

            var tenantRestrictedPermission = productAndOrganisationIsRideProtectOrUserFromDefaultOrganisation
                ? Permission.ViewAllQuotesFromAllOrganisations : (Permission?)null;

            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                quoteOrganisationId,
                ownerId,
                customerId,
                "quote",
                resourceId,
                Permission.ViewQuotes,
                Permission.ViewAllQuotes,
                tenantRestrictedPermission);
        }

        private async Task ThrowIfUserHasNoViewCustomerPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid organisationId,
            Guid? ownerId,
            Guid customerId,
            string resourceId)
        {
            await this.ThrowIfUserHasNoPermission(
                tenantId,
                performingUser,
                organisationId,
                ownerId,
                customerId,
                "customer",
                resourceId,
                Permission.ViewCustomers,
                Permission.ViewAllCustomers,
                Permission.ViewAllCustomersFromAllOrganisations);
        }

        private void ThrowIfUserIsNotAgent(
            IUserAuthenticationData userAuthenticationData,
            string? action = null)
        {
            if (userAuthenticationData.UserType != UserType.Client)
            {
                string userType = userAuthenticationData.UserType.ToString().ToLower();
                throw new ErrorException(Errors.General.Forbidden(
                    action,
                    $"{userType} user accounts are not permitted to perform this action"));
            }
        }

        /// <summary>
        /// Throw if the performing user doesnt have the right permission to access.
        /// </summary>
        /// <param name="tenantId">The records tenant id.</param>
        /// <param name="performingUser">The performing user id to authenticate with.</param>
        /// <param name="recordOrganisationId">The records organisation id.</param>
        /// <param name="ownerId">The records owner id.</param>
        /// <param name="customerId">The records customer id.</param>
        /// <param name="resourceName">The resource name to show in error message.</param>
        /// <param name="resourceId">The resource Id to show in error message.</param>
        /// <param name="ownerRestrictedPermission">The weakest permission example ViewClaims.</param>
        /// <param name="organisationRestrictedPermission">The middle strongest permission, example: ViewAllClaims.</param>
        /// <param name="tenantRestrictedPermission">The strongest permission. ViewAllClaimsFromAllOrganisations.</param>
        private async Task ThrowIfUserHasNoPermission(
            Guid tenantId,
            ClaimsPrincipal performingUser,
            Guid? recordOrganisationId,
            Guid? ownerId,
            Guid? customerId,
            string resourceName,
            string? resourceId,
            Permission? ownerRestrictedPermission,
            Permission? organisationRestrictedPermission,
            Permission? tenantRestrictedPermission)
        {
            var isPerformingUserFromDefaultOrganisation =
               await this.organisationService.IsOrganisationDefaultForTenant(
                   performingUser.GetTenantId(), performingUser.GetOrganisationId());

            bool isPerformingUserFromRideProtectOrganisation =
              await this.IsPerformingUserFromRideProtectOrganisation(performingUser.GetTenantId(), performingUser.GetOrganisationId());

            bool hasPermission = tenantRestrictedPermission != null
                ? await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, tenantRestrictedPermission.Value))
                : false;
            if (hasPermission
                && (isPerformingUserFromDefaultOrganisation || isPerformingUserFromRideProtectOrganisation))
            {
                if (performingUser.GetTenantId() == tenantId || performingUser.IsMasterUser())
                {
                    return;
                }
                else if (tenantRestrictedPermission != null)
                {
                    // you don't have the permission to view/manage, sorry.
                    throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                        tenantRestrictedPermission.Value,
                        resourceName,
                        resourceId,
                        "user must be in the same tenant"));
                }
            }
            else if (organisationRestrictedPermission != null
                && await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, organisationRestrictedPermission.Value)))
            {
                if (performingUser.GetOrganisationId() == recordOrganisationId || recordOrganisationId == null)
                {
                    return;
                }
                else
                {
                    var message = tenantRestrictedPermission != null
                        ? $"user must be in the same organisation, or you need the \"{tenantRestrictedPermission.Value.Humanize()}\" permission"
                        : $"user must be in the same organisation, or you need a higher level permission";

                    // you don't have the permission to view/manage, sorry.
                    throw new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                        organisationRestrictedPermission.Value,
                        resourceName,
                        resourceId,
                        message));
                }
            }
            else if (ownerRestrictedPermission != null && await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, ownerRestrictedPermission.Value))
                   &&
                   ((performingUser.GetUserType() == UserType.Client && ownerId == performingUser.GetId())
                   || (performingUser.GetUserType() == UserType.Customer && customerId == performingUser.GetCustomerId())))
            {
                return;
            }
            else
            {
                // you don't have the basic permission to view/manage, sorry.
                throw new ErrorException(Errors.General.NotAuthorized($"access {resourceName} because you dont have any permission,"
                    + " or you dont own the record"));
            }
        }

        // this is a temporary hardcode condition for ride-protect sub-organisation,
        // to be removed after the implementation of UB-8372
        private async Task<bool> ProductAndOrganisationIsRideProtectOrPerformingUserFromDefaultOrganisation(ClaimsPrincipal performingUser, Guid tenantId, Guid? productId)
        {
            var isPerformingUserFromDefaultOrganisation = false;
            var isPerformingUserFromRideProtectOrganisation = false;

            if (performingUser != null && performingUser.IsAuthenticated())
            {
                isPerformingUserFromDefaultOrganisation =
                    await this.organisationService.IsOrganisationDefaultForTenant(performingUser.GetTenantId(), performingUser.GetOrganisationId());
                isPerformingUserFromRideProtectOrganisation = await this.IsPerformingUserFromRideProtectOrganisation(performingUser.GetTenantId(), performingUser.GetOrganisationId());
            }

            var product = await this.cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(productId));
            var isRideProtectProduct = product?.Details?.Alias == "ride-protect";
            return (isPerformingUserFromRideProtectOrganisation && isRideProtectProduct) || isPerformingUserFromDefaultOrganisation;
        }

        // this is a temporary hardcode condition for ride-protect sub-organisation,
        // to be removed after the implementation of UB-8372
        private async Task<bool> IsPerformingUserFromRideProtectOrganisation(Guid tenantId, Guid organisationId)
        {
            // this is a temporary hardcode condition for ride-protect sub-organisation,
            // to be removed after the implementation of UB-8372
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
            return organisation?.Alias == "ride-protect";
        }

        // this is a temporary hardcode condition for ride-protect sub-organisation,
        // to be removed after the implementation of UB-8372
        private async Task UpdateFilterConditionForRideProtectOrganisation(Guid tenantId, EntityListFilters filters)
        {
            var productIds = new List<Guid>();
            var product = await this.cachingResolver.GetProductByAliasOrThrow(tenantId, "ride-protect");
            productIds.Add(product.Id);

            filters.IsRideProtectOrganisation = true;
            filters.RideProtectProductId = product.Id;
        }

        private async Task ThrowIfUserHasNoPermissionOnReport(ClaimsPrincipal performingUser, Guid reportId, Permission requiredPermission)
        {
            ReportReadModel? reportDetails = this.reportReadModelRepository.SingleOrDefaultIncludeAllProperties(performingUser.GetTenantId(), reportId);
            reportDetails = EntityHelper.ThrowIfNotFound(reportDetails, reportId, "report");
            bool isAllowed = await this.mediator.Send(new PrincipalHasPermissionQuery(performingUser, requiredPermission))
                || reportDetails.OrganisationId != performingUser.GetOrganisationId();
            if (!isAllowed)
            {
                throw new ErrorException(Errors.General.NotAuthorized());
            }
        }
    }
}
