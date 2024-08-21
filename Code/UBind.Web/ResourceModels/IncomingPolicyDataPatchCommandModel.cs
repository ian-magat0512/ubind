// <copyright file="IncomingPolicyDataPatchCommandModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Services.PolicyDataPatcher;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Json;
    using UBind.Web.Exceptions;

    /// <summary>
    /// Model for form data patch command.
    /// </summary>
    public class IncomingPolicyDataPatchCommandModel : PolicyDataPatchCommandModel
    {
        /// <summary>
        /// Generate a form data patch command.
        /// </summary>
        /// <returns>Instnace of form data patch command to use.</returns>
        public PolicyDataPatchCommand ToCommand()
        {
            if (this.TargetFormDataPath == null
                && this.TargetCalculationResultPath == null)
            {
                throw new BadRequestException(
                    $"Patch must specify at least one of {nameof(this.TargetFormDataPath)} or {nameof(this.TargetCalculationResultPath)}");
            }

            if (this.Type.Equals(PatchCommandType.CopyField))
            {
                if (this.SourceEntity == PatchSourceEntity.None)
                {
                    throw new BadRequestException($"Copy patch must specify {nameof(this.SourceEntity)}.");
                }

                if (this.SourcePath == null)
                {
                    throw new BadRequestException($"Copy patch must specify {nameof(this.SourcePath)}.");
                }

                if (this.NewValue != null)
                {
                    throw new BadRequestException($"Copy patch must not specify {nameof(this.NewValue)}.");
                }

                return new CopyFieldPolicyDataPatchCommand(
                    ParseJsonPath(this.TargetFormDataPath),
                    ParseJsonPath(this.TargetCalculationResultPath),
                    this.SourceEntity,
                    ParseJsonPath(this.SourcePath),
                    this.CreateScope(),
                    this.GetRules());
            }

            if (this.Type.Equals(PatchCommandType.GivenValue))
            {
                if (this.NewValue == null)
                {
                    throw new BadRequestException($"give value patch must specify {nameof(this.NewValue)}.");
                }

                if (this.SourceEntity != PatchSourceEntity.None)
                {
                    throw new BadRequestException($"give value patch must not specify {nameof(this.SourceEntity)}.");
                }

                if (this.SourcePath != null)
                {
                    throw new BadRequestException($"Given value patch must not specify {nameof(this.SourcePath)}.");
                }

                return new GivenValuePolicyDataPatchCommand(
                    ParseJsonPath(this.TargetFormDataPath),
                    ParseJsonPath(this.TargetCalculationResultPath),
                    ParseNewValue(this.NewValue),
                    this.CreateScope(),
                    this.GetRules());
            }

            throw new BadRequestException($"Unsupported patch command type: {this.Type}.");
        }

        private static JToken ParseNewValue(string newValue)
        {
            if (newValue == null)
            {
                return null;
            }

            try
            {
                return JToken.Parse(newValue);
            }
            catch (JsonException)
            {
                throw new BadRequestException($@"<{newValue}> is not a valid json value. String values must include double quotes.");
            }
        }

        private static JsonPath ParseJsonPath(string jsonPath)
        {
            try
            {
                return new JsonPath(jsonPath);
            }
            catch (JsonPathFormatException ex)
            {
                throw new BadRequestException($"The patch command contained an invalid JSON path: {ex.Message}.");
            }
        }

        private PolicyDataPatchScope CreateScope()
        {
            if (this.ScopeType == PatchScopeType.Global)
            {
                if (this.ScopeEntityId != default)
                {
                    throw new BadRequestException("ScopeEntityId must not be set when scope is global.");
                }

                if (this.ScopeVersionNumber != 0)
                {
                    throw new BadRequestException("ScopeVersionNumber must not be set when scope is global.");
                }

                return PolicyDataPatchScope.CreateGlobalPatchScope();
            }

            if (this.ScopeEntityId == default)
            {
                throw new BadRequestException("ScopeEntityId must be set when scope is not global.");
            }

            if (this.ScopeType == PatchScopeType.QuoteVersion)
            {
                if (this.ScopeVersionNumber == 0)
                {
                    throw new BadRequestException("ScopeVersionNumber must be non-zero when patch is scoped to a particular quote version.");
                }

                return PolicyDataPatchScope.CreateQuoteVersionPatchScope(this.ScopeEntityId, this.ScopeVersionNumber);
            }

            return this.ScopeType == PatchScopeType.QuoteFull
                ? PolicyDataPatchScope.CreateFullQuotePatchScope(this.ScopeEntityId)
                : this.ScopeType == PatchScopeType.QuoteLatest
                    ? PolicyDataPatchScope.CreateLatestQuotePatchScope(this.ScopeEntityId)
                    : PolicyDataPatchScope.CreatePolicyTransactionPatchScope(this.ScopeEntityId);
        }

        private PatchRules GetRules()
        {
            var rules = PatchRules.None;
            foreach (var rule in this.Rules)
            {
                rules |= rule;
            }

            if (!rules.IsValidCombination())
            {
                throw new BadRequestException("Patch includes contradictory rules.");
            }

            return rules;
        }
    }
}
