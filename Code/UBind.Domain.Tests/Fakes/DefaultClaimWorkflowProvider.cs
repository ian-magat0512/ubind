// <copyright file="DefaultClaimWorkflowProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Product;

    public class DefaultClaimWorkflowProvider : IClaimWorkflowProvider
    {
        public async Task<IClaimWorkflow> GetConfigurableClaimWorkflow(ReleaseContext releaseContext)
        {
            return await Task.FromResult(new DefaultClaimWorkflow());
        }
    }
}
