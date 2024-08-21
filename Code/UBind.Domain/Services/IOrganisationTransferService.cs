// <copyright file="IOrganisationTransferService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Threading.Tasks;
    using Hangfire.Server;

    /// <summary>
    /// Service responsible of processing a user or a customer to transfer to another organisation.
    /// </summary>
    public interface IOrganisationTransferService
    {
        /// <summary>
        /// Validates the user data for organisation transfer.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant in <see cref="Guid"/>.</param>
        /// <param name="userId">The ID of the user in <see cref="Guid"/>.</param>
        /// <param name="destinationOrganisationId">The destination ID of the organisation in <see cref="Guid"/>.</param>
        Task ValidateUserDataForTransfer(Guid tenantId, Guid userId, Guid destinationOrganisationId);

        /// <summary>
        /// Validates the customer data for organisation transfer.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant in <see cref="Guid"/>.</param>
        /// <param name="customerId">The ID of the customer in <see cref="Guid"/>.</param>
        /// <param name="destinationOrganisationId">The destination ID of the organisation in <see cref="Guid"/>.</param>
        void ValidateCustomerDataForTransfer(Guid tenantId, Guid customerId, Guid destinationOrganisationId);

        /// <summary>
        /// Transfer the person and user aggregates, including its login and emails to another organisation.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="currentOrganisationId">The current organisation ID of the user in <see cref="Guid"/>.</param>
        /// <param name="destinationOrganisationId">The destination ID of the organisation in <see cref="Guid"/>.</param>
        /// <param name="performingUserId">The ID of the performing user in <see cref="Guid"/>.</param>
        /// <returns>A task that can be awaited.</returns>
        Task TransferUserAggregateIncludingLoginAndEmailsToAnotherOrganisation(Guid tenantId, Guid userId, Guid currentOrganisationId, Guid destinationOrganisationId, Guid? performingUserId);

        /// <summary>
        /// Transfer the customer to another organisation within the same tenancy.
        /// </summary>
        /// <remarks>
        /// The transfer includes login, customer, quotes, claims, policies and reports.
        /// </remarks>
        /// <param name="tenantId">The ID of the tenant in <see cref="Guid"/>.</param>
        /// <param name="customerId">The ID of the customer in <see cref="Guid"/>.</param>
        /// <param name="destinationOrganisationId">The destination ID of the organisation in <see cref="Guid"/>.</param>
        /// <param name="performingUserId">The ID of the performing user in <see cref="Guid"/>.</param>
        /// <param name="performContext">The hangfire parameter context.</param>
        Task TransferCustomerToAnotherOrganisation(Guid tenantId, Guid customerId, Guid destinationOrganisationId, Guid? performingUserId, PerformContext performContext);
    }
}
