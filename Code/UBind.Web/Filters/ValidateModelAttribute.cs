// <copyright file="ValidateModelAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Domain;
    using UBind.Web.Extensions;

    /// <summary>
    /// Filter attribute for triggering model validation.
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        /// <inheritdoc/>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = Errors.General.ModelValidationFailed(
                    null,
                    context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)).ToProblemJsonResult();
            }
        }
    }
}
