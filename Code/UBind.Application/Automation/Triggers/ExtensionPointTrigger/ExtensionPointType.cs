// <copyright file="ExtensionPointType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers.ExtensionPointTrigger
{
    /// <summary>
    /// Enumeration of extension point trigger types.
    /// </summary>
    public enum ExtensionPointType
    {
        // an extension point for assigning a policy number
        AssignPolicyNumber,

        // for generating policy number
        GeneratePolicyNumber,

        // for before quote calculations
        BeforeQuoteCalculation,

        // for preparing quote form input
        PrepareQuoteFormInputData,

        // for after quote calculations
        AfterQuoteCalculation,
    }
}
