// <copyright file="ProgressStepIcons.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Stores what icons should be used when progress step is in a given state.
    /// Properties are in the order of precedence.
    /// </summary>
    public class ProgressStepIcons
    {
        public string Active { get; set; }

        public string First { get; set; }

        public string Last { get; set; }

        public string Future { get; set; }

        public string Past { get; set; }

        public string Inactive { get; set; }
    }
}
