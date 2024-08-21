﻿// <copyright file="OptionalYearModelBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Mapping.ThirdPartyDataSets.GlassGuide;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using UBind.Domain.Exceptions;

/// <summary>
/// The model binder for Glass's Guide optional "Year" parameter.
/// </summary>
public class OptionalYearModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var valueAsString = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(valueAsString))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
        }
        else if (int.TryParse(valueAsString, out int result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            throw new ErrorException(Domain.Errors.ThirdPartyDataSets.GlassGuide.YearGroupFormatInvalidOptionalValue(valueAsString));
        }

        return Task.CompletedTask;
    }
}
