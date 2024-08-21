// <copyright file="RequestIntentAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Attributes
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// An attribute that can be applied to a controller action to specify the intent of the request.
    /// This helps determine if the whole request should be read-only or read-write.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestIntentAttribute : Attribute
    {
        public RequestIntentAttribute(RequestIntent requestIntent)
        {
            this.RequestIntent = requestIntent;
        }

        public RequestIntent RequestIntent { get; private set; }
    }
}
