// <copyright file="IContentSecurityPolicyConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Configuration
{
    public interface IContentSecurityPolicyConfiguration
    {
        public FormsAppConfiguration FormsApp { get; }

        public PortalAppConfiguration PortalApp { get; }
    }
}