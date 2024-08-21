// <copyright file="GivenValuePolicyDataPatchCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;

    /// <summary>
    /// A patch command that includes a specific value to be set on the patched property.
    /// </summary>
    public class GivenValuePolicyDataPatchCommand : PolicyDataPatchCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GivenValuePolicyDataPatchCommand"/> class.
        /// </summary>
        /// <param name="targetFormDataPath">The path of the property in the form data to patch.</param>
        /// <param name="targetCalculationResultPath">The path of the property in the calculation result to patch.</param>
        /// <param name="newValue">The new value for the property.</param>
        /// <param name="scope">The scope of the patch, specifying which entities to patch within the policy.</param>
        /// <param name="rules">The condition that must hold for the patch to be applied.</param>
        public GivenValuePolicyDataPatchCommand(
            JsonPath targetFormDataPath,
            JsonPath targetCalculationResultPath,
            JToken newValue,
            PolicyDataPatchScope scope,
            PatchRules rules)
            : base(targetFormDataPath, targetCalculationResultPath, scope, rules)
        {
            this.NewValue = newValue;
        }

        /// <summary>
        /// Gets the value that is to be set.
        /// </summary>
        public JToken NewValue { get; }

        /// <inheritdoc/>
        public override JToken GetNewValue(QuoteAggregate quoteAggregate)
        {
            return this.NewValue;
        }
    }
}
