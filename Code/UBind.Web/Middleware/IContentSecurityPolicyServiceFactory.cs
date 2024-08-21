// <copyright file="IContentSecurityPolicyServiceFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware
{
    public interface IContentSecurityPolicyServiceFactory
    {
        IContentSecurityPolicyService Create(AppType contentType);
    }
}
