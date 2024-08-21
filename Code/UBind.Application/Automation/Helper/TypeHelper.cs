// <copyright file="TypeHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper;

public static class TypeHelper
{
    public static string GetReadableTypeName(object value)
    {
        if (DataObjectHelper.IsArray(value))
        {
            return "list";
        }

        if (DataObjectHelper.IsObject(value))
        {
            return "object";
        }

        if (value is byte)
        {
            return "byte";
        }

        if (value is bool)
        {
            return "condition";
        }

        if (value is long)
        {
            return "integer";
        }

        if (value is decimal || value is double)
        {
            return "number";
        }

        if (value is string)
        {
            return "text";
        }

        return null;
    }
}
