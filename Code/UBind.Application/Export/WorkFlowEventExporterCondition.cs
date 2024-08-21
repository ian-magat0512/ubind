// <copyright file="WorkFlowEventExporterCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Text;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// Factory for workflow step conditions.
    /// </summary>
    public class WorkFlowEventExporterCondition : EventExporterCondition
    {
        private readonly string workFlowStep;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkFlowEventExporterCondition"/> class.
        /// </summary>
        /// <param name="workFlowStep">The current workflow step.</param>
        public WorkFlowEventExporterCondition(string workFlowStep = "")
        {
            this.workFlowStep = workFlowStep;
        }

        /// <inheritdoc/>
        public override Task<bool> Evaluate(ApplicationEvent applicationEvent)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var quote = applicationEvent.Aggregate.GetQuoteBySequenceNumber(applicationEvent.EventSequenceNumber);
            var conditionMet = this.workFlowStep == quote.WorkflowStep;
            stringBuilder.Append("When working whether to workflow event matches, the specified workflow step was ");
            stringBuilder.Append($"{this.workFlowStep} and the actual workflow step was ");
            stringBuilder.Append($"{quote.WorkflowStep} ");
            stringBuilder.Append("so the condition " + (conditionMet ? "WAS MET" : "WAS NOT MET"));

            this.DebugInfo = stringBuilder.ToString();
            return Task.FromResult(conditionMet);
        }
    }
}
