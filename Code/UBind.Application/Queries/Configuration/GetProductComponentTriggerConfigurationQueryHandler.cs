// <copyright file="GetProductComponentTriggerConfigurationQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Configuration
{
    using StackExchange.Profiling;
    using UBind.Application.Queries.ProductConfiguration;
    using UBind.Application.Releases;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// Query Handler for getting the product trigger configuration.
    /// </summary>
    public class GetProductComponentTriggerConfigurationQueryHandler : IQueryHandler<GetProductComponentTriggerConfigurationQuery, List<Trigger>>
    {
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IConfigurationService configurationService;

        public GetProductComponentTriggerConfigurationQueryHandler(
            IReleaseQueryService releaseQueryService,
            IConfigurationService configurationService)
        {
            this.releaseQueryService = releaseQueryService;
            this.configurationService = configurationService;
        }

        /// <inheritdoc/>
        public async Task<List<Trigger>> Handle(GetProductComponentTriggerConfigurationQuery request, CancellationToken cancellationToken)
        {
            using (MiniProfiler.Current.Step(nameof(GetProductComponentTriggerConfigurationQueryHandler) + "." + nameof(this.Handle)))
            {
                var release = this.releaseQueryService.GetRelease(request.ReleaseContext);
                ProductComponentConfiguration componentConfig =
                    release.GetProductComponentConfigurationOrThrow(request.WebFormAppType);
                return await Task.FromResult(componentConfig.Component.Triggers);
            }
        }
    }
}
