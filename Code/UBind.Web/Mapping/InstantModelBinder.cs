// <copyright file="InstantModelBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mapping
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using NodaTime;

    /// <summary>
    /// The instant model binder for strings.
    /// </summary>
    public class InstantModelBinder : IModelBinder
    {
        /// <summary>
        /// Bind the input to the model.
        /// </summary>
        /// <param name="bindingContext">The binding context.</param>
        /// <returns>If task completed.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType != typeof(Instant))
            {
                return Task.CompletedTask;
            }

            var val = bindingContext.ValueProvider.GetValue(bindingContext.OriginalModelName);
            if (val == default)
            {
                throw new ArgumentNullException("Date should be in ISO 8601 format.");
            }

            var rawValue = val.FirstValue as string;
            if (DateTime.TryParse(rawValue, out DateTime dateTime))
            {
                Instant instant = Instant.FromDateTimeUtc(dateTime.ToUniversalTime());
                bindingContext.Model = instant;
                bindingContext.Result = ModelBindingResult.Success(instant);
            }

            return Task.CompletedTask;
        }
    }
}
