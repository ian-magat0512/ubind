// <copyright file="CrnGenerationConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using Newtonsoft.Json;

    /// <summary>
    /// Specification for CRN generation.
    /// </summary>
    public class CrnGenerationConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrnGenerationConfiguration"/> class.
        /// </summary>
        /// <param name="isUniqueAcrossTenant">A value indicating if the CRN should be unique across all products in a tenant.</param>
        /// <param name="generationMethod">The method for generating CRNs.</param>
        /// <param name="prefix">The prefix to be used in CRN generation, if any, otherwise null.</param>
        public CrnGenerationConfiguration(
            bool isUniqueAcrossTenant,
            CrnGenerationMethod generationMethod,
            string prefix = null)
        {
            this.IsUniqueAcrossTenant = isUniqueAcrossTenant;
            this.Method = generationMethod;
            this.Prefix = prefix;
        }

        [JsonConstructor]
        private CrnGenerationConfiguration()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the CRN should be unique across all products in a tenant.
        /// </summary>
        [JsonProperty]
        public bool IsUniqueAcrossTenant { get; private set; }

        /// <summary>
        /// Gets the method for generating the CRN.
        /// </summary>
        [JsonProperty]
        public CrnGenerationMethod Method { get; private set; }

        /// <summary>
        /// Gets the prefix to use when generating CRNs, if required, otherwise null.
        /// </summary>
        [JsonProperty]
        public string Prefix { get; private set; }
    }
}
