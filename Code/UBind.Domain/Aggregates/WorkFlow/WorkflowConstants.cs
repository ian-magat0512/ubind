// <copyright file="WorkflowConstants.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Workflow
{
    using System.Collections.Generic;

    /// <summary>
    /// Holds values for known constants used in the state machine
    /// workflow implementation.
    /// </summary>
    public class WorkflowConstants
    {
        /// <summary>
        /// Gets the customer operation value;.
        /// </summary>
        public static string CustomerOperation => "Customer";

        /// <summary>
        /// Gets the quote version operation value;.
        /// </summary>
        public static string QuoteVersionOperation => "QuoteVersion";

        /// <summary>
        /// Gets the calculation operation value;.
        /// </summary>
        public static string CalculationOperation => "Calculation";

        /// <summary>
        /// Gets the form update operation value.
        /// </summary>
        public static string FormDataUpdateOperation => "FormUpdate";

        /// <summary>
        /// Gets a list of quote states that do not allow data update actions.
        /// </summary>
        public static List<string> NoDataUpdateStates => new List<string> { "Declined", "Complete" };
    }
}
