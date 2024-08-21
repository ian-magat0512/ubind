// <copyright file="TransferCustomerToOtherOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using MediatR;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Command handler for transferring the existing customer to another organisation and must be of the same tenancy. It includes
    /// the customer's login data for customer user, emails, quotes, policies and claims.
    /// </summary>
    public class TransferCustomerToOtherOrganisationCommandHandler
        : ICommandHandler<TransferCustomerToOtherOrganisationCommand>
    {
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IOrganisationTransferService organisationTransferService;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly Guid? performingUserId;
        private readonly IUserSessionDeletionService userSessionDeletionService;

        public TransferCustomerToOtherOrganisationCommandHandler(
            ICustomerReadModelRepository customerReadModelRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IOrganisationTransferService organisationTransferService,
            IBackgroundJobClient backgroundJobClient,
            IUserSessionDeletionService userSessionDeletionService)
        {
            this.customerReadModelRepository = customerReadModelRepository;
            this.performingUserId = httpContextPropertiesResolver.PerformingUserId;
            this.organisationTransferService = organisationTransferService;
            this.backgroundJobClient = backgroundJobClient;
            this.userSessionDeletionService = userSessionDeletionService;
        }

        public async Task<Unit> Handle(TransferCustomerToOtherOrganisationCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.organisationTransferService.ValidateCustomerDataForTransfer(command.TenantId, command.CustomerId, command.OrganisationId);

            this.backgroundJobClient.Enqueue<IOrganisationTransferService>(
                organisationTransferService => organisationTransferService.TransferCustomerToAnotherOrganisation(
                    command.TenantId, command.CustomerId, command.OrganisationId, this.performingUserId, null));

            var customer = this.customerReadModelRepository.GetCustomerById(command.TenantId, command.CustomerId);
            if (customer.UserId.HasValue && customer.UserHasBeenActivated)
            {
                // Delete the customer user's token so he has to log in again.
                await this.userSessionDeletionService.DeleteForUser(customer.TenantId, customer.UserId.Value);
            }

            return Unit.Value;
        }
    }
}
