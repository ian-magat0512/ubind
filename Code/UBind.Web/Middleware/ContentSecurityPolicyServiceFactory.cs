// <copyright file="ContentSecurityPolicyServiceFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware
{
    using UBind.Web.Configuration;

    public class ContentSecurityPolicyServiceFactory : IContentSecurityPolicyServiceFactory
    {
        private readonly IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration;

        public ContentSecurityPolicyServiceFactory(IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration)
        {
            this.contentSecurityPolicyConfiguration = contentSecurityPolicyConfiguration;
        }

        public IContentSecurityPolicyService Create(AppType contentType)
        {
            switch (contentType)
            {
                case AppType.Portal:
                    return new PortalAppContentSecurityPolicyService(this.contentSecurityPolicyConfiguration);
                case AppType.Forms:
                    return new FormsAppContentSecurityPolicyService(this.contentSecurityPolicyConfiguration);
                default:
                    throw new ArgumentException("Invalid content type");
            }
        }
    }
}
