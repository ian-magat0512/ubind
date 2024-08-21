// <copyright file="RequiredFieldHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper;

using Newtonsoft.Json.Linq;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Exceptions;

public static class RequiredFieldHelper
{
    public static async Task ThrowIfRequiredActionPropertyIsNull(object value, string propertyName, IProviderContext providerContext)
    {
        if (value == null)
        {
            JObject errorData = await providerContext.GetDebugContext();
            var actionType = errorData.SelectToken(ErrorDataKey.ActionType);
            throw new ErrorException(Errors.Automation.RequiredActionParameterValueMissing(actionType?.ToString(), propertyName, errorData));
        }
    }
}
