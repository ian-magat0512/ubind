// <copyright file="ContentSecurityPolicyConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Configuration
{
    public class ContentSecurityPolicyConfiguration : IContentSecurityPolicyConfiguration
    {
        public ContentSecurityPolicyConfiguration()
        {
            this.FormsApp = new FormsAppConfiguration();
            this.PortalApp = new PortalAppConfiguration();
        }

        public FormsAppConfiguration FormsApp { get; set; }

        public PortalAppConfiguration PortalApp { get; set; }
    }

    public class FormsAppConfiguration
    {
        public FormsAppConfiguration()
        {
            this.DefaultDomains = new List<string>();
            this.FrameAncestorDomains = new List<string>();
            this.ScriptDomains = new List<string>();
            this.StyleDomains = new List<string>();
        }

        public List<string> DefaultDomains { get; private set; }

        public List<string> FrameAncestorDomains { get; private set; }

        public List<string> ScriptDomains { get; private set; }

        public List<string> StyleDomains { get; private set; }
    }

    public class PortalAppConfiguration
    {
        public PortalAppConfiguration()
        {
            this.DefaultDomains = new List<string>();
            this.FrameAncestorDomains = new List<string>();
            this.ScriptDomains = new List<string>();
            this.StyleDomains = new List<string>();
        }

        public List<string> DefaultDomains { get; set; }

        public List<string> FrameAncestorDomains { get; set; }

        public List<string> ScriptDomains { get; set; }

        public List<string> StyleDomains { get; set; }
    }
}
