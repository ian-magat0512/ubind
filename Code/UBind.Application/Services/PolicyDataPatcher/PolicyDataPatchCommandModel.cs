// <copyright file="PolicyDataPatchCommandModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.PolicyDataPatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Json;

    /// <summary>
    /// Model for form data patch command.
    /// </summary>
    public abstract class PolicyDataPatchCommandModel
    {
        /// <summary>
        /// Gets the patch command type.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public PatchCommandType Type { get; private set; }

        /// <summary>
        /// Gets the target form data path of the model.
        /// </summary>
        [JsonProperty]
        public string TargetFormDataPath { get; private set; }

        /// <summary>
        /// Gets the target calculation result path of the model.
        /// </summary>
        [JsonProperty]
        public string TargetCalculationResultPath { get; private set; }

        /// <summary>
        /// Gets the rules restricting application of the patch.
        /// </summary>
        [JsonProperty]
        public IEnumerable<PatchRules> Rules { get; private set; } = Enumerable.Empty<PatchRules>();

        /// <summary>
        /// Gets the source entity of the model.
        /// </summary>
        [JsonProperty]
        public PatchSourceEntity SourceEntity { get; private set; }

        /// <summary>
        /// Gets the source path of the model.
        /// </summary>
        [JsonProperty]
        public string SourcePath { get; private set; }

        /// <summary>
        /// Gets the new value of the form data patch.
        /// </summary>
        [JsonProperty]
        public string NewValue { get; private set; }

        /// <summary>
        /// Gets the type of the scope.
        /// </summary>
        [JsonProperty]
        public PatchScopeType ScopeType { get; private set; }

        /// <summary>
        /// Gets the ID of the entity the patch is scoped to, if not global, otherwise default.
        /// </summary>
        [JsonProperty]
        public Guid ScopeEntityId { get; private set; }

        /// <summary>
        /// Gets the number of the quote version the patch is scoped to, of scoped to a quote version, otherwise zero.
        /// </summary>
        [JsonProperty]
        public int ScopeVersionNumber { get; private set; }
    }
}
