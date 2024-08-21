// <copyright file="ContentSecurityPolicyBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Helpers;

using System.Collections.Generic;
using System.Text;

/// <summary>
/// A helper class that encapsulates the Content Security Policy implementations.
/// </summary>
public class ContentSecurityPolicyBuilder
{
    private List<string> scriptSrcDomains;
    private List<string> frameAncestorsDomains;
    private List<string> styleSrcDomains;
    private List<string> defaultSrcDomains;

    public ContentSecurityPolicyBuilder()
    {
        this.scriptSrcDomains = new List<string>();
        this.frameAncestorsDomains = new List<string>();
        this.styleSrcDomains = new List<string>();
        this.defaultSrcDomains = new List<string>();
    }

    /// <summary>
    /// Adds a trusted domain as a script src.
    /// </summary>
    /// <param name="domain">The trusted domain script src to be added.</param>
    public void AddScriptSrcDomain(string domain)
    {
        this.scriptSrcDomains.Add(domain);
    }

    /// <summary>
    /// Adds a trusted frame ancestor domain.
    /// </summary>
    /// <param name="domain">The trusted frame ancestor domain to be added.</param>
    public void AddFrameAncestorsDomain(string domain)
    {
        this.frameAncestorsDomains.Add(domain);
    }

    /// <summary>
    /// Adds a trusted domain as a style src.
    /// </summary>
    /// <param name="domain">The trusted frame ancestor domain to be added.</param>
    public void AddStyleSrcDomain(string domain)
    {
        this.styleSrcDomains.Add(domain);
    }

    /// <summary>
    /// Adds a trusted domain as a default src.
    /// </summary>
    /// <param name="domain">The trusted frame ancestor domain to be added.</param>
    public void AddDefaultSrcDomain(string domain)
    {
        this.defaultSrcDomains.Add(domain);
    }

    /// <summary>
    /// Generate the CSP policy based on all the passed domains.
    /// </summary>
    /// <returns>returns policy value for CSP.</returns>
    public string GetPolicyString()
    {
        var policyBuilder = new StringBuilder();

        if (this.scriptSrcDomains.Count > 0)
        {
            policyBuilder.Append("script-src ");
            policyBuilder.Append(string.Join(" ", this.scriptSrcDomains));
            policyBuilder.Append("; ");
        }

        if (this.frameAncestorsDomains.Count > 0)
        {
            policyBuilder.Append("frame-ancestors ");
            policyBuilder.Append(string.Join(" ", this.frameAncestorsDomains));
            policyBuilder.Append("; ");
        }
        else
        {
            policyBuilder.Append("frame-ancestors 'none'; ");
        }

        if (this.styleSrcDomains.Count > 0)
        {
            policyBuilder.Append("style-src ");
            policyBuilder.Append(string.Join(" ", this.styleSrcDomains));
            policyBuilder.Append("; ");
        }

        if (this.defaultSrcDomains.Count > 0)
        {
            policyBuilder.Append("default-src ");
            policyBuilder.Append(string.Join(" ", this.defaultSrcDomains));
            policyBuilder.Append("; ");
        }

        policyBuilder.Append("upgrade-insecure-requests; ");
        policyBuilder.Append("block-all-mixed-content; ");

        return policyBuilder.ToString();
    }
}