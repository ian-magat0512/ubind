// <copyright file="ContentField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Represents the configuration for a field that only displays content and does not have any interaction with the
    /// user.
    /// </summary>
    [WorkbookFieldType("None")]
    [JsonFieldType("content")]
    public class ContentField : VisibleField
    {
    }
}
