// <copyright file="WebFormValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// A helper class that encapsulates the validation rules to be followed by all webform-app operations.
    /// </summary>
    public static class WebFormValidator
    {
        /// <summary>
        /// Validates if the quote being requested as resource matches the tenant, product and environment parameters
        /// passed as part of the requested resource.
        /// If not, it throws an exception.
        /// </summary>
        /// <param name="quoteId">The ID of the quote being requested.</param>
        /// <param name="quote">The quote being requested.</param>
        /// <param name="productContext">The product context that was used in the request.</param>
        public static void ValidateQuoteRequest(
            Guid quoteId, QuoteAggregate quote, IProductContext productContext)
        {
            ValidateRequest(quote, quoteId, productContext, "quote");
        }

        public static void ValidateQuoteRequest(
            Guid quoteId, NewQuoteReadModel quote, IProductContext productContext)
        {
            ValidateRequest(quote, quoteId, productContext, "quote");
        }

        /// <summary>
        /// Verifies if the claim being requested matches the tenant, product and environment parameters passed as part of
        /// the requested resource.
        /// If not, this throws an exception.
        /// </summary>
        /// <param name="claimId">The ID of the claim being requested.</param>
        /// <param name="claim">The claim being requested.</param>
        /// <param name="productContext">The product context that was used in the request.</param>
        public static void ValidateClaimRequest(
            Guid claimId, ClaimAggregate claim, IProductContext productContext)
        {
            ValidateRequest(claim, claimId, productContext, "claim");
        }

        public static void ValidateClaimRequest(
            Guid claimId, ClaimReadModel claim, IProductContext productContext)
        {
            var claimProductContext = new ProductContext(claim.TenantId, claim.ProductId, claim.Environment);
            ValidateRequest(claimProductContext, claimId, productContext, "claim");
        }

        private static void ValidateRequest(
            IProductContext entityContext,
            Guid entityId,
            IProductContext productContext,
            string entityType)
        {
            if (entityContext == null)
            {
                throw new NotFoundException(Errors.General.NotFound(entityType, entityId));
            }

            if (entityContext.TenantId != productContext.TenantId || entityContext.ProductId != productContext.ProductId)
            {
                throw new ErrorException(Errors.General.Forbidden($"access a {entityType} from a different tenancy or product"));
            }

            if (entityContext.Environment != productContext.Environment)
            {
                throw new ErrorException(Errors.Operations.EnvironmentMisMatch(entityType));
            }
        }
    }
}
