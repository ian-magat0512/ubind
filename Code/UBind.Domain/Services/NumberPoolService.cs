// <copyright file="NumberPoolService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReferenceNumbers;

    /// <summary>
    /// Provides service to add, remove and retrive numbers in number pools.
    /// </summary>
    public class NumberPoolService : INumberPoolService
    {
        private readonly IInvoiceNumberRepository invoiceNumberRepository;
        private readonly ICreditNoteNumberRepository creditNoteNumberRepository;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IClaimNumberRepository claimNumberRepository;

        public NumberPoolService(
            IInvoiceNumberRepository invoiceNumberRepository,
            ICreditNoteNumberRepository creditNoteNumberRepository,
            IPolicyNumberRepository policyNumberRepository,
            IClaimNumberRepository claimNumberRepository)
        {
            this.invoiceNumberRepository = invoiceNumberRepository;
            this.creditNoteNumberRepository = creditNoteNumberRepository;
            this.policyNumberRepository = policyNumberRepository;
            this.claimNumberRepository = claimNumberRepository;
        }

        /// <inheritdoc/>
        public NumberPoolAddResult Add(Guid tenantId, Guid productId, string numberPoolId, DeploymentEnvironment environment, IEnumerable<string> numbers)
        {
            return this.GetRepositoryForPool(numberPoolId)
                .LoadForProduct(tenantId, productId, environment, numbers);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAvailable(Guid tenantId, Guid productId, string numberPoolId, DeploymentEnvironment environment)
        {
            return this.GetRepositoryForPool(numberPoolId)
                .GetAvailableForProduct(tenantId, productId, environment);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAll(Guid tenantId, Guid productId, string numberPoolId, DeploymentEnvironment environment)
        {
            return this.GetRepositoryForPool(numberPoolId).GetAllForProduct(
                tenantId, productId, environment);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> Remove(Guid tenantId, Guid productId, string numberPoolId, DeploymentEnvironment environment, IEnumerable<string> numbers)
        {
            return this.GetRepositoryForPool(numberPoolId)
                .DeleteForProduct(tenantId, productId, environment, numbers);
        }

        private INumberPoolRepository GetRepositoryForPool(string numberPoolId)
        {
            switch (numberPoolId.ToEnumOrThrow<NumberPool>())
            {
                case NumberPool.Invoice:
                    return this.invoiceNumberRepository;
                case NumberPool.CreditNote:
                    return this.creditNoteNumberRepository;
                case NumberPool.Policy:
                    return this.policyNumberRepository;
                case NumberPool.Claim:
                    return this.claimNumberRepository;
                default:
                    throw new ErrorException(Errors.General.NotFound("number pool", numberPoolId));
            }
        }
    }
}
