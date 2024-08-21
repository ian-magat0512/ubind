// <copyright file="UpdatePolicyNumberCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    public class UpdatePolicyNumberCommandHandler : IRequestHandler<UpdatePolicyNumberCommand, IPolicyReadModelDetails>
    {
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly ISystemAlertService systemAlertService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IPolicyService policyService;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IUBindDbContext dbContext;

        public UpdatePolicyNumberCommandHandler(
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IPolicyNumberRepository policyNumberRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            ISystemAlertService systemAlertService,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IPolicyService policyService,
            IPolicyReadModelRepository policyReadModelRepository,
            IUBindDbContext dbContext)
        {
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.policyNumberRepository = policyNumberRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.systemAlertService = systemAlertService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.policyService = policyService;
            this.policyReadModelRepository = policyReadModelRepository;
            this.dbContext = dbContext;
        }

        public async Task<IPolicyReadModelDetails> Handle(UpdatePolicyNumberCommand request, CancellationToken cancellationToken)
        {
            var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForPolicy(request.TenantId, request.PolicyId);
            await this.policyService.ThrowIfPolicyNumberInUse(
                request.TenantId, quoteAggregate.ProductId, request.Environment, request.NewPolicyNumber);
            var oldPolicyNumber = quoteAggregate.Policy?.PolicyNumber;
            string newPolicyNumber = string.Empty;
            var productContext = new ProductContext(
                request.TenantId,
                quoteAggregate.ProductId,
                request.Environment);
            try
            {
                if (string.IsNullOrWhiteSpace(request.NewPolicyNumber))
                {
                    var consumedPolicyNumber = this.policyNumberRepository.ConsumeAndSave(productContext);
                    newPolicyNumber = consumedPolicyNumber;
                    await this.systemAlertService.QueuePolicyNumberThresholdAlertCheck(
                        request.TenantId,
                        quoteAggregate.ProductId,
                        request.Environment);
                }
                else
                {
                    newPolicyNumber = this.policyNumberRepository.ConsumePolicyNumber(
                        request.TenantId,
                        quoteAggregate.ProductId,
                        request.NewPolicyNumber,
                        request.Environment);
                }

                if (request.ReturnOldPolicyNumberToPool)
                {
                    if (string.IsNullOrEmpty(oldPolicyNumber))
                    {
                        throw new InvalidOperationException($"Cannot reuse policy number with no value.");
                    }

                    this.policyNumberRepository.ReturnOldPolicyNumberToPool(
                        request.TenantId,
                        quoteAggregate.ProductId,
                        oldPolicyNumber,
                        request.Environment);
                }
                else
                {
                    if (string.IsNullOrEmpty(oldPolicyNumber))
                    {
                        throw new InvalidOperationException($"Cannot unassign policy number with no value.");
                    }

                    this.policyNumberRepository.DeletePolicyNumber(
                        request.TenantId,
                        quoteAggregate.ProductId,
                        oldPolicyNumber,
                        request.Environment);
                }

                quoteAggregate.Policy.UpdatePolicyNumber(newPolicyNumber, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                await this.quoteAggregateRepository.Save(quoteAggregate);
            }
            catch (Exception ex)
            {
                this.policyService.UnConsumePolicyNumberAndPersist(productContext, newPolicyNumber);
                throw;
            }
            return this.policyReadModelRepository
                .GetPolicyDetails(request.TenantId, request.PolicyId);
        }
    }
}
