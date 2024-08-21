// <copyright file="DataType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel;

    /// <summary>
    /// Represents the data type of a field, which is the type of data it generates.
    /// </summary>
    public enum DataType
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        [Description("abn")]
        Abn,
        [Description("acn")]
        Acn,
        [Description("attachment")]
        Attachment,
        [Description("boolean")]
        Boolean,
        [Description("currency")]
        Currency,
        [Description("date")]
        Date,
        [Description("email")]
        Email,
        [Description("name")]
        Name,
        [Description("none")]
        None,
        [Description("number")]
        Number,
        [Description("number plate")]
        NumberPlate,
        [Description("password")]
        Password,
        [Description("percent")]
        Percent,
        [Description("phone")]
        Phone,
        [Description("postcode")]
        Postcode,
        [Description("repeating")]
        Repeating,
        [Description("text")]
        Text,
        [Description("time")]
        Time,
        [Description("url")]
        Url,
#pragma warning restore SA1602 // Enumeration items should be documented
    }
}
