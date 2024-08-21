// <copyright file="PolicyDataPatchCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;

    /// <summary>
    /// For representing commands for patching form data.
    /// </summary>
    public abstract class PolicyDataPatchCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDataPatchCommand"/> class.
        /// </summary>
        /// <param name="targetFormDataPath">The path of the form data property to patch.</param>
        /// <param name="targetCalculationResultPath">The path of the calculation result property to patch.</param>
        /// <param name="scope">The scope of the patch, specifying which entities to patch within the policy.</param>
        /// <param name="rules">The rules to determine whether a patch should be applied to target data.</param>
        protected PolicyDataPatchCommand(
            JsonPath targetFormDataPath,
            JsonPath targetCalculationResultPath,
            PolicyDataPatchScope scope,
            PatchRules rules)
        {
            this.TargetFormDataPath = targetFormDataPath;
            this.TargetCalculationResultPath = targetCalculationResultPath;
            this.Scope = scope;
            this.Rules = rules;
        }

        /// <summary>
        /// Gets the path of the property in the form data that is to be patched.
        /// </summary>
        public JsonPath TargetFormDataPath { get; }

        /// <summary>
        /// Gets the path of the property in the calculation result that is to be patched.
        /// </summary>
        public JsonPath TargetCalculationResultPath { get; }

        /// <summary>
        /// Gets the scope of the patch specifies which entities within the policy should be patched.
        /// </summary>
        public PolicyDataPatchScope Scope { get; }

        /// <summary>
        /// Gets the rules that are used to see if a patch can be applied.
        /// </summary>
        public PatchRules Rules { get; }

        /// <summary>
        /// Method for selecting the new value to be set on the patched property.
        /// </summary>
        /// <param name="quoteAggregate">The policy the patch is being applied to.</param>
        /// <returns>A JToken representing the new value for the property.</returns>
        public abstract JToken GetNewValue(QuoteAggregate quoteAggregate);
    }
}
