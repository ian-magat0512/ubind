// <copyright file="IReferenceNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// an interface for reference numbers.
    /// </summary>
    public interface IReferenceNumber
    {
        /// <summary>
        /// Gets the ID of the tenant the reference number is for.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the ID of the product the reference number is for.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the deployment environment the reference number is for.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets the reference number for assignment.
        /// </summary>
        string Number { get; }

        /// <summary>
        /// Gets a value indicating whether a reference number has already been used.
        /// </summary>
        bool IsAssigned { get; }

        /// <summary>
        /// Use the reference number tagging the object as assigned.
        /// </summary>
        /// <returns>The reference number.</returns>
        string Consume();

        /// <summary>
        /// Use the reference number tagging the object as Unassigned.
        /// </summary>
        void UnConsume();
    }
}
