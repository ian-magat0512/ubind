// <copyright file="DuplicateEntryNumberKeyExceptionParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Text.RegularExpressions;
    using UBind.Domain;

    /// <summary>
    /// For extracting duplicate policy or invoice number from exception message.
    /// </summary>
    public class DuplicateEntryNumberKeyExceptionParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateEntryNumberKeyExceptionParser"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the duplicate is expected to be for.</param>
        /// <param name="productId">The ID of the product the duplicate is expected to be for.</param>
        /// <param name="environment">The environment the duplicate is expected to be for.</param>
        /// <param name="message">The exception message to be parsed.</param>
        public DuplicateEntryNumberKeyExceptionParser(Guid tenantId, Guid productId, DeploymentEnvironment environment, string message)
        {
            var regex = new Regex($"The duplicate key value is \\({tenantId}, {productId}, {(int)environment}, (.+)\\)\\.");
            var match = regex.Match(message);
            if (match.Success)
            {
                this.Succeeded = true;
                this.Number = match.Groups[1].Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the parsing succeeded.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the duplicate policy number.
        /// </summary>
        public string Number { get; }
    }
}
