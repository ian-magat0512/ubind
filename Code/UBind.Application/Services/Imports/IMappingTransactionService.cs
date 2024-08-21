// <copyright file="IMappingTransactionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Imports;
    using UBind.Domain.Loggers;

    /// <summary>
    /// Represents the mapping transaction service.
    /// </summary>
    public interface IMappingTransactionService
    {
        /// <summary>
        /// Creates a new customer user record.
        /// </summary>
        /// <param name="logger">The logger than handles progress report to the requester.</param>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="data">Represents the customer import data object.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<CustomerAggregate> HandleCustomers(IProgressLogger logger, ImportBaseParam baseParam, CustomerImportData data, bool updateEnabled = false);

        /// <summary>
        /// Creates a new quote and policy record.
        /// </summary>
        /// <param name="logger">The logger than handles progress report to the requester.</param>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="data">Represents the policy import data object.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandlePolicies(IProgressLogger logger, ImportBaseParam baseParam, PolicyImportData data, bool updateEnabled = false);

        /// <summary>
        /// Creates a new claim record.
        /// </summary>
        /// <param name="logger">The logger than handles progress report to the requester.</param>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="data">Represents the claim import data object.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleClaims(IProgressLogger logger, ImportBaseParam baseParam, ClaimImportData data, bool updateEnabled = false);

        Task HandleQuotes(IProgressLogger logger, ImportBaseParam baseParam, QuoteImportData data, bool updateEnabled = false);
    }
}
