// <copyright file="DirectDebit.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// Iqumulate Premium Funding Response Direct Debit.
    /// </summary>
    public class DirectDebit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectDebit"/> class.
        /// </summary>
        [JsonConstructor]
        public DirectDebit()
        {
        }

        /// <summary>
        /// Gets the Account Name.
        /// </summary>
        [JsonProperty]
        public string AccountName { get; private set; }

        /// <summary>
        /// Gets the Account number.
        /// </summary>
        [JsonProperty]
        public string AccountNumber { get; private set; }

        /// <summary>
        /// Gets the BSB.
        /// </summary>
        [JsonProperty]
        public string BSB { get; private set; }
    }
}
