// <copyright file="GetAutomationsForEnvironmentQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.PortalExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Automation;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    public class GetAutomationsForEnvironmentQueryHandler
        : IQueryHandler<GetAutomationsForEnvironmentQuery, List<ProductAutomation>>
    {
        private readonly IAutomationConfigurationProvider configurationProvider;
        private readonly IProductRepository productRepository;

        public GetAutomationsForEnvironmentQueryHandler(
            IAutomationConfigurationProvider configurationProvider,
            IProductRepository productRepository)
        {
            this.configurationProvider = configurationProvider;
            this.productRepository = productRepository;
        }

        public async Task<List<ProductAutomation>> Handle(
            GetAutomationsForEnvironmentQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // cache this for 10min, as this doesnt change much,
            // but is called all the time, and its slow.
            var automations = await MemoryCachingHelper.AddOrGetAsync(
                  "portalFeature+TenantId:" + request.TenantId + "|Environment:" + request.Environment,
                  async () => await this.GetAutomations(request.TenantId, request.Environment),
                  DateTimeOffset.Now.AddMinutes(10));
            return automations;
        }

        private async Task<List<ProductAutomation>> GetAutomations(Guid tenantId, DeploymentEnvironment environment)
        {
            var automations = new List<ProductAutomation>();
            var products = this.productRepository.GetDeployedActiveProducts(tenantId, environment).ToList();
            foreach (var prd in products)
            {
                var config = await this.configurationProvider.GetAutomationConfigurationOrNull(
                    prd.TenantId,
                    prd.Id,
                    environment,
                    null);
                if (config == null)
                {
                    continue;
                }

                automations.AddRange(config.Automations
                    .Select(a => new ProductAutomation(prd.Id, prd.Details.Alias, environment, a)));
            }
            return automations;
        }
    }
}
