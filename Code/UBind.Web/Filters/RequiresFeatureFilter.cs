// <copyright file="RequiresFeatureFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System.Linq;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc.Filters;
    using StackExchange.Profiling;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.FeatureSettings;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Web.Extensions;

    public class RequiresFeatureFilter : IActionFilter
    {
        private ICqrsMediator mediator;

        public RequiresFeatureFilter(ICqrsMediator mediator)
        {
            this.mediator = mediator;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // noop
        }

        /// <inheritdoc/>
        public async void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.User == null || !context.HttpContext.User.IsAuthenticated())
            {
                return;
            }

            var tenantId = context.HttpContext.User.GetTenantIdOrNull();
            var userId = context.HttpContext.User.GetId();

            if (tenantId == null || !userId.HasValue)
            {
                return;
            }

            // Get the RequiresFeature attribute from the currently executing action method
            var requiresFeatureAttributes = context.ActionDescriptor.FilterDescriptors
                .Select(x => x.Filter).OfType<RequiresFeatureAttribute>();
            using (MiniProfiler.Current.Step("RequiresFeatureFilter.OnActionExecuting"))
            {
                if (requiresFeatureAttributes.Any())
                {
                    foreach (var requiresFeatureAttribute in requiresFeatureAttributes)
                    {
                        var command = new UserHasActiveFeatureSettingQuery(context.HttpContext.User, requiresFeatureAttribute.Feature);
                        var hasActiveFeature = await this.mediator.Send(command);

                        if (!hasActiveFeature)
                        {
                            context.Result = Errors.General.FeatureDisabled(
                                requiresFeatureAttribute.Feature.Humanize()).ToProblemJsonResult();

                            break;
                        }
                    }
                }
            }
        }
    }
}
