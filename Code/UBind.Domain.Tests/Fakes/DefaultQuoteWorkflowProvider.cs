// <copyright file="DefaultQuoteWorkflowProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System.Threading.Tasks;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Product;

    /// <summary>
    /// The default quote workflow provider for test files.
    /// </summary>
    public class DefaultQuoteWorkflowProvider : IQuoteWorkflowProvider
    {
        /// <inheritdoc/>
        public Task<IQuoteWorkflow> GetConfigurableQuoteWorkflow(ReleaseContext releaseContext)
        {
            IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();
            return Task.FromResult(quoteWorkflow);
        }
    }
}
