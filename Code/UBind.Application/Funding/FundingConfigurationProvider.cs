// <copyright file="FundingConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding
{
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using StackExchange.Profiling;
    using UBind.Application.Releases;
    using UBind.Domain.Product;

    /// <inheritdoc />
    public class FundingConfigurationProvider : IFundingConfigurationProvider
    {
        private readonly IReleaseQueryService releaseQueryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FundingConfigurationProvider"/> class.
        /// </summary>
        /// <param name="releaseQueryService">Service for obtaining the current product release.</param>
        public FundingConfigurationProvider(IReleaseQueryService releaseQueryService) =>
            this.releaseQueryService = releaseQueryService;

        /// <inheritdoc/>
        public async Task<Maybe<IFundingConfiguration>> GetConfigurationAsync(ReleaseContext releaseContext)
        {
            using (MiniProfiler.Current.Step(nameof(FundingConfigurationProvider) + "." + nameof(this.GetConfigurationAsync)))
            {
                var release = this.releaseQueryService.GetRelease(releaseContext);
                return await Task.FromResult(release.FundingConfigurationModel);
            }
        }
    }
}
