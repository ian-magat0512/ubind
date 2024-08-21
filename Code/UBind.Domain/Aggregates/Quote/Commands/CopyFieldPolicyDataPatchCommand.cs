// <copyright file="CopyFieldPolicyDataPatchCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    using System;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;

    /// <summary>
    /// Command for copying data from one field to another.
    /// </summary>
    public class CopyFieldPolicyDataPatchCommand : PolicyDataPatchCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFieldPolicyDataPatchCommand"/> class.
        /// </summary>
        /// <param name="targetFormDataPath">The path of the form data property to patch.</param>
        /// <param name="targetCalculationResultPath">The path of the calculation result property to patch.</param>
        /// <param name="sourceEntity">The entity to obtain the new value from.</param>
        /// <param name="sourcePath">The path of the form data property to obtain the new entity from.</param>
        /// <param name="scope">The scope of the patch, specifying which entities to patch within the policy.</param>
        /// <param name="rules">The condition that must hold for the patch to be applied.</param>
        public CopyFieldPolicyDataPatchCommand(
            JsonPath targetFormDataPath,
            JsonPath targetCalculationResultPath,
            PatchSourceEntity sourceEntity,
            JsonPath sourcePath,
            PolicyDataPatchScope scope,
            PatchRules rules)
            : base(targetFormDataPath, targetCalculationResultPath, scope, rules)
        {
            this.SourceEntity = sourceEntity;
            this.SourcePath = sourcePath;
        }

        /// <summary>
        /// Gets a value indicating where to copy the field from.
        /// </summary>
        public PatchSourceEntity SourceEntity { get; }

        /// <summary>
        /// Gets the path of the field to copy.
        /// </summary>
        public JsonPath SourcePath { get; }

        /// <inheritdoc/>
        public override JToken GetNewValue(QuoteAggregate quoteAggregate)
        {
            if (this.SourceEntity == PatchSourceEntity.FirstPolicyTransactionCalculationResult)
            {
                var transaction = quoteAggregate.Policy.Transactions
                    .OfType<Entities.PolicyTransaction>()
                    .FirstOrDefault();
                if (transaction == null)
                {
                    throw new InvalidOperationException("Could not find any policy transaction of type PolicyUpsertTransaction");
                }

                var calculationResult = JObject.Parse(transaction.CalculationResult.Json);
                return calculationResult.SelectToken(this.SourcePath.Value);
            }

            throw new NotImplementedException("Only copying from policy transaction calculation results is supported so far.");
        }
    }
}
