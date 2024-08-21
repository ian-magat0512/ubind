// <copyright file="EnumModelBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mapping
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This is used to automatically parse strings to enums when specified as action parameters
    /// on controller methods, using our own string extension ToEnum method. The reason to use
    /// this would be if the standard AspNetCoreMvc converter of enums isn't sufficient for you,
    /// e.g. because it doesn't convert using the description attribute.
    /// To use this, prefix the param with the following:
    /// [ModelBinder(BinderType = typeof(EnumModelBinder))]
    /// .
    /// </summary>
    public class EnumModelBinder : IModelBinder
    {
        /// <inheritdoc/>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (!bindingContext.ModelType.IsEnum)
            {
                return Task.CompletedTask;
            }

            var val = bindingContext.ValueProvider.GetValue(bindingContext.OriginalModelName);

            // if the value is null and the type is nullable, return null
            bool isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
            if ((val == ValueProviderResult.None || val.FirstValue == null) && isNullable)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            var rawValue = val.FirstValue as string;
            Enum enumValue = rawValue.ToEnumOrThrow(bindingContext.ModelType);
            bindingContext.Model = enumValue;
            bindingContext.Result = ModelBindingResult.Success(enumValue);
            return Task.CompletedTask;
        }
    }
}
