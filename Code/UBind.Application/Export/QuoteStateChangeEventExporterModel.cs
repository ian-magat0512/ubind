// <copyright file="QuoteStateChangeEventExporterModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for conditional exporter.
    /// </summary>
    public class QuoteStateChangeEventExporterModel : IExporterModel<EventExporterCondition>
    {
        /// <summary>
        /// Gets or sets the text provider for providing the operation name.
        /// </summary>
        public IExporterModel<ITextProvider> OperationName { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the original state.
        /// </summary>
        public IExporterModel<ITextProvider> OriginalState { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the resulting state.
        /// </summary>
        public IExporterModel<ITextProvider> ResultingState { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the current workflow step.
        /// </summary>
        public IExporterModel<ITextProvider> CurrentWorkflowStep { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the user type.
        /// </summary>
        public IExporterModel<ITextProvider> UserType { get; set; }

        /// <summary>
        /// Build the condition.
        /// </summary>
        /// <param name="dependencyProvider">Container for dependencies required for exporter building.</param>
        /// <param name="productConfiguration">Contains product configuration.</param>
        /// <returns>Event exporter condition.</returns>
        public EventExporterCondition Build(IExporterDependencyProvider dependencyProvider, IProductConfiguration productConfiguration)
        {
            var condition = new QuoteStateChangeEventExporterCondition(
                this.OperationName?.Build(dependencyProvider, productConfiguration),
                this.OriginalState?.Build(dependencyProvider, productConfiguration),
                this.ResultingState?.Build(dependencyProvider, productConfiguration),
                this.UserType?.Build(dependencyProvider, productConfiguration),
                this.CurrentWorkflowStep?.Build(dependencyProvider, productConfiguration),
                dependencyProvider.UserService);
            return condition;
        }
    }
}
