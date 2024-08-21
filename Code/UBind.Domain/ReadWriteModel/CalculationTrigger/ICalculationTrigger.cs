// <copyright file="ICalculationTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel.CalculationTrigger
{
    /// <summary>
    /// The calculation trigger interface.
    /// </summary>
    public interface ICalculationTrigger
    {
        /// <summary>
        /// Gets the name of the trigger.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the error message of the trigger.
        /// </summary>
        string ErrorMessage { get; }
    }
}
