// <copyright file="AggregateType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents various types of aggregates.
    /// </summary>
    public enum AggregateType
    {
        User = 0,
        Person = 1,
        Customer = 2,
        Quote = 3,
        Claim = 4,
        Organisation = 5,
        Report = 6,
        TextAdditionalPropertyValue = 7,
        AdditionalPropertyDefinition = 8,
        FinancialTransaction = 9,
        Portal = 10,
        StructuredDataAdditionalPropertyValue = 11,
    }
}
