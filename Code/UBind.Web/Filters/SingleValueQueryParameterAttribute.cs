// <copyright file="SingleValueQueryParameterAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Filters
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Mvc.Filters;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SingleValueQueryParameterAttribute : ActionFilterAttribute
    {
        private readonly string[] singleValueParameters;

        public SingleValueQueryParameterAttribute(params string[] singleValueParameters)
        {
            this.singleValueParameters = singleValueParameters;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var query = context.HttpContext.Request.Query;
            foreach (var parameterName in this.singleValueParameters)
            {
                this.ThrowIfParameterIsSetAlready(query, parameterName);
            }

            base.OnActionExecuting(context);
        }

        private void ThrowIfParameterIsSetAlready(IQueryCollection query, string parameterName)
        {
            if (query.ContainsKey(parameterName) && query[parameterName].Count > 1)
            {
                string details = "When trying to process your request, the attempt failed because the parameter" +
                $" \"{parameterName.ToCamelCase()}\" has been set more than once. To resolve the issue," +
                $" please ensure that the parameter \"{parameterName.ToCamelCase()}\" is set only once in the request." +
                " If you require further assistance please contact technical support.";
                throw new ErrorException(new Error(
                    $"request.parameter.invalid",
                    $"A request parameter has been set more than once",
                    details,
                    HttpStatusCode.BadRequest));
            }
        }
    }
}