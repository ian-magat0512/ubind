// <copyright file="TransferUserToOtherOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for transferring the existing user to another organisation and must be of the same tenancy. It includes
    /// the user's login data, customers, emails, quotes, policies and claims.
    /// </summary>
    public class TransferUserToOtherOrganisationCommandHandler
        : ICommandHandler<TransferUserToOtherOrganisationCommand, Unit>
    {
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IOrganisationTransferService organisationTransferService;
        private readonly IOrganisationService organisationService;
        private readonly IUserService userService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly Guid? performingUserId;
        private readonly IUserSessionDeletionService userSessionDeletionService;

        public TransferUserToOtherOrganisationCommandHandler(
            IUserReadModelRepository userReadModelRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IRoleRepository roleRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IOrganisationTransferService organisationTransferService,
            IOrganisationService organisationService,
            IUserService userService,
            IBackgroundJobClient backgroundJobClient,
            IUserSessionDeletionService userSessionDeletionService)
        {
            this.performingUserId = httpContextPropertiesResolver.PerformingUserId;
            this.userReadModelRepository = userReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.roleRepository = roleRepository;
            this.organisationTransferService = organisationTransferService;
            this.organisationService = organisationService;
            this.userService = userService;
            this.backgroundJobClient = backgroundJobClient;
            this.userSessionDeletionService = userSessionDeletionService;
        }

        public Task<Unit> Handle(TransferUserToOtherOrganisationCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.organisationTransferService.ValidateUserDataForTransfer(request.TenantId, request.UserId, request.ToOrganisationId);
            var user = this.userReadModelRepository.GetUser(request.TenantId, request.UserId);

            var mainJobId = string.Empty;
            if (request.IncludeCustomers)
            {
                mainJobId = this.backgroundJobClient.Enqueue<TransferUserToOtherOrganisationCommandHandler>(
                    service => service.EnqueueTransferUserCustomersToAnotherOrganisation(request.TenantId, request.ToOrganisationId, user.Id));
            }

            if (string.IsNullOrEmpty(mainJobId))
            {
                this.backgroundJobClient.Enqueue<TransferUserToOtherOrganisationCommandHandler>(
                    service => service.EnqueueTransferUserDetailsToAnotherOrganisation(
                        request.TenantId, user.OrganisationId, request.ToOrganisationId, user.Id, request.IncludeCustomers));
            }
            else
            {
                this.backgroundJobClient.ContinueJobWith<TransferUserToOtherOrganisationCommandHandler>(
                    mainJobId, service => service.EnqueueTransferUserDetailsToAnotherOrganisation(
                        request.TenantId, user.OrganisationId, request.ToOrganisationId, user.Id, request.IncludeCustomers));
            }

            return Task.FromResult(Unit.Value);
        }

        public async Task SwapUserRole(Guid tenantId, Guid userId, Guid? performingUserId)
        {
            var user = this.userReadModelRepository.GetUser(tenantId, userId);

            var tenantAdminRole = this.roleRepository.GetRoleByNameOrThrow(user.TenantId, "Tenant Admin");
            var organisationAdminRole = this.roleRepository.GetRoleByNameOrThrow(user.TenantId, "Organisation Admin");

            var sourceIsDefaultOrganisation = await this.organisationService.IsOrganisationDefaultForTenant(user.TenantId, user.OrganisationId);
            if (sourceIsDefaultOrganisation && user.Roles.Contains(tenantAdminRole))
            {
                await this.userService.SwapRoleForUsers(user.TenantId, performingUserId, tenantAdminRole, organisationAdminRole, new List<UserReadModel> { user });
            }
        }

        public void EnqueueTransferUserCustomersToAnotherOrganisation(Guid tenantId, Guid organisationId, Guid userId)
        {
            var user = this.userReadModelRepository.GetUser(tenantId, userId);
            var filter = new EntityListFilters()
            {
                OrganisationIds = new Guid[] { user.OrganisationId },
                OwnerUserId = user.Id,
            };
            var userCustomersToBeTransferred = this.customerReadModelRepository.GetCustomersSummaryMatchingFilters(user.TenantId, filter).ToList();

            foreach (var customer in userCustomersToBeTransferred)
            {
                this.backgroundJobClient.Enqueue<IOrganisationTransferService>(
                    service => service.TransferCustomerToAnotherOrganisation(tenantId, customer.Id, organisationId, this.performingUserId, null));
            }
        }

        public void EnqueueTransferUserDetailsToAnotherOrganisation(
            Guid tenantId, Guid organisationId, Guid destinationOrganisationId, Guid userId, bool includeCustomers = true)
        {
            this.backgroundJobClient.Enqueue<TransferUserToOtherOrganisationCommandHandler>(service => service.SwapUserRole(tenantId, userId, this.performingUserId));
            this.backgroundJobClient.Enqueue<IOrganisationTransferService>(
                service => service.TransferUserAggregateIncludingLoginAndEmailsToAnotherOrganisation(tenantId, userId, organisationId, destinationOrganisationId, this.performingUserId));
            this.backgroundJobClient.Enqueue<TransferUserToOtherOrganisationCommandHandler>(
                service => this.LogoutTransferredUsers(tenantId, destinationOrganisationId, userId, includeCustomers));
        }

        public async Task LogoutTransferredUsers(Guid tenantId, Guid organisationId, Guid userId, bool includeCustomers = true)
        {
            // Delete the user's token so he has to log in again.
            await this.userSessionDeletionService.DeleteForUser(tenantId, userId);

            if (includeCustomers)
            {
                // Delete all the customer user's token
                var entityFilterList = new EntityListFilters()
                {
                    OrganisationIds = new Guid[] { organisationId },
                    OwnerUserId = userId,
                };
                var userActiveCustomers = this.customerReadModelRepository.GetActiveCustomerUsersSummaryMatchingFilters(tenantId, entityFilterList).ToList();
                foreach (var customer in userActiveCustomers)
                {
                    await this.userSessionDeletionService.DeleteForUser(tenantId, customer.UserId.Value);
                }
            }
        }
    }
}
