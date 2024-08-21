// <copyright file="RequestIntentMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using UBind.Domain.Attributes;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Attempts to detect the intent of the current request (i.e. read-only or read-write) and an item on the
    /// HttpContext, so that when an instance of <see cref="IUBindDbContext"/> is created, it can set that intent,
    /// so the read only queries are directed to the read-only replica.
    /// </summary>
    public class RequestIntentMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        private readonly List<string> readonlyVerbs = new List<string> { "GET", "HEAD", "OPTIONS" };

        public RequestIntentMiddleware(
            RequestDelegate next,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            RequestIntent? requestIntent = null;
            var endpoint = httpContext.GetEndpoint();
            var controllerActionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (controllerActionDescriptor != null)
            {
                var controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();
                var actionMethod = controllerActionDescriptor.MethodInfo;
                RequestIntentAttribute? requestIntentAttribute
                    = controllerActionDescriptor.MethodInfo
                        .GetCustomAttributes<RequestIntentAttribute>()
                        .FirstOrDefault();
                if (requestIntentAttribute != null)
                {
                    requestIntent = requestIntentAttribute.RequestIntent;
                }
            }

            // check the http verb from the actual request
            if (requestIntent == null)
            {
                requestIntent = RequestIntent.ReadWrite;
                string httpVerb = httpContext.Request.Method;
                foreach (var readonlyVerb in this.readonlyVerbs)
                {
                    if (httpVerb.EqualsIgnoreCase(readonlyVerb))
                    {
                        requestIntent = RequestIntent.ReadOnly;
                        break;
                    }
                }
            }

            httpContext.Items[nameof(RequestIntent)] = requestIntent;
            await this.next(httpContext);
        }
    }
}
