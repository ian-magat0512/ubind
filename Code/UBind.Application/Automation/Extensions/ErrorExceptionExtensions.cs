// <copyright file="ErrorExceptionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Extensions
{
    using System.Collections.Generic;
    using System.Runtime.ExceptionServices;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Exceptions;

    public static class ErrorExceptionExtensions
    {
        public static void EnrichAndRethrow(this ErrorException ex, JObject? jObject = null, List<string>? additionalDetails = null)
        {
            if (jObject != null)
            {
                if (ex.Error.Data != null)
                {
                    ex.Error.Data.Merge(jObject);
                }
                else
                {
                    ex.Error.Data = jObject;
                }
            }

            if (additionalDetails != null)
            {
                if (ex.Error.AdditionalDetails != null)
                {
                    ex.Error.AdditionalDetails.AddRange(additionalDetails);
                }
                else
                {
                    ex.Error.AdditionalDetails = additionalDetails;
                }
            }

            ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }
}
