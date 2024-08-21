// <copyright file="QuoteAggregateResolverService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Services
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// This service is needed to get a QuoteAggregate or QuoteAggregateId for a give quoteId.
    /// </summary>
    public class QuoteAggregateResolverService : IQuoteAggregateResolverService
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteAggregateResolverService"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        /// <param name="quoteAggregateRepository">The quote aggregate repository.</param>
        public QuoteAggregateResolverService(
            IQuoteReadModelRepository quoteReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
        }

        /// <inheritdoc/>
        public Guid GetQuoteAggregateIdForQuoteIdOrPolicyId(Guid quoteOrPolicyId)
        {
            try
            {
                return this.GetQuoteAggregateIdForQuoteId(quoteOrPolicyId);
            }
            catch (ErrorException ex) when (ex.Error?.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return this.GetQuoteAggregateIdForPolicyId(quoteOrPolicyId);
            }
        }

        /// <inheritdoc/>
        public Guid GetQuoteAggregateIdForQuoteId(Guid quoteId)
        {
            var quoteAggregateId = this.quoteReadModelRepository.GetQuoteAggregateId(quoteId);
            if (quoteAggregateId == null)
            {
                throw new ErrorException(Errors.General.NotFound("quote", quoteId));
            }

            return (Guid)quoteAggregateId;
        }

        /// <inheritdoc/>
        public Guid GetQuoteAggregateIdForPolicyId(Guid policyId)
        {
            var quoteAggregateId = this.policyReadModelRepository.GetQuoteAggregateId(policyId);
            if (quoteAggregateId == default)
            {
                throw new ErrorException(Errors.General.NotFound("policy", policyId));
            }

            return quoteAggregateId;
        }

        /// <inheritdoc/>
        public QuoteAggregate GetQuoteAggregateForQuote(Guid tenantId, Guid quoteId)
        {
            var quoteAggregateId = this.GetQuoteAggregateIdForQuoteId(quoteId);
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
            if (quoteAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("quote aggregate", quoteAggregateId));
            }

            return quoteAggregate;
        }

        public QuoteAggregate GetQuoteAggregateForPolicy(Guid tenantId, Guid policyId)
        {
            var quoteAggregateId = this.GetQuoteAggregateIdForPolicyId(policyId);
            var quoteAggregate = this.quoteAggregateRepository.GetById(tenantId, quoteAggregateId);
            if (quoteAggregate == null)
            {
                throw new ErrorException(Errors.General.NotFound("policy", policyId));
            }

            return quoteAggregate;
        }
    }
}
