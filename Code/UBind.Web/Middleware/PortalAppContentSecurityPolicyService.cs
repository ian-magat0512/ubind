// <copyright file="PortalAppContentSecurityPolicyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using UBind.Application.Authorisation;
using UBind.Domain;
using UBind.Domain.Helpers;
using UBind.Domain.ReadModel.Portal;
using UBind.Web.Configuration;
using UBind.Web.Helpers;

public class PortalAppContentSecurityPolicyService : IContentSecurityPolicyService
{
    private readonly IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration;
    private ContentSecurityPolicyBuilder contentSecurityPolicy;

    public PortalAppContentSecurityPolicyService(IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration)
    {
        this.contentSecurityPolicyConfiguration = contentSecurityPolicyConfiguration;
        this.contentSecurityPolicy = new ContentSecurityPolicyBuilder();
    }

    public async Task<string?> GetPolicyHeaderValue(HttpContext httpContext, ICachingResolver cachingResolver, IAuthorisationService authorisationService)
    {
        var hasPortal = httpContext.Request.Query.TryGetValue("portal", out StringValues portal);
        var startsWithPortal = httpContext.Request.Path.StartsWithSegments("/portal");
        if (!startsWithPortal || !hasPortal)
        {
            return string.Empty;
        }

        PortalReadModel? portalModel = await this.GetPortalModel(httpContext, cachingResolver, portal);
        if (portalModel == null || portalModel.Disabled)
        {
            return string.Empty;
        }

        return this.GetContentSecurityPolicyHeaderValue(portalModel);
    }

    private async Task<PortalReadModel?> GetPortalModel(
        HttpContext httpContext,
        ICachingResolver cachingResolver,
        StringValues portal)
    {
        string path = httpContext.Request.Path;
        path = path.TrimStart(new char[] { '/' });
        string[] pathSegments = path.Split(new char[] { '/' });
        if (pathSegments.Length < 2)
        {
            return null;
        }

        string tenant = pathSegments[1];
        var tenantModel = await cachingResolver.GetTenantOrNull(new GuidOrAlias(tenant));
        if (tenantModel == null)
        {
            return null;
        }

        return await cachingResolver.GetPortalOrNull(tenantModel.Id, new GuidOrAlias(portal.ToString()));
    }

    private string GetContentSecurityPolicyHeaderValue(PortalReadModel portal)
    {
        this.AddFrameAncestorDomains(portal);
        this.AddScriptSourceDomains();
        this.AddStyleSourceDomains();
        this.AddDefaultSourceDomains();
        return this.contentSecurityPolicy.GetPolicyString();
    }

    private void AddStyleSourceDomains()
    {
        this.contentSecurityPolicy.AddStyleSrcDomain("'self'");
        this.contentSecurityPolicy.AddStyleSrcDomain("'unsafe-inline'");
        foreach (var url in this.contentSecurityPolicyConfiguration.PortalApp.StyleDomains)
        {
            this.contentSecurityPolicy.AddStyleSrcDomain(url);
        }
    }

    private void AddFrameAncestorDomains(PortalReadModel portal)
    {
        var deploymentTargets = new List<string>();
        string[] urls =
        {
            portal.ProductionUrl ?? string.Empty,
            portal.StagingUrl ?? string.Empty,
            portal.DevelopmentUrl ?? string.Empty,
        };

        foreach (var url in urls)
        {
            if (!string.IsNullOrEmpty(url))
            {
                deploymentTargets.Add(this.GetSchemeAndDomain(url));
            }
        }

        this.contentSecurityPolicy.AddFrameAncestorsDomain("'self'");
        foreach (var url in this.contentSecurityPolicyConfiguration.PortalApp.FrameAncestorDomains)
        {
            this.contentSecurityPolicy.AddFrameAncestorsDomain(url);
        }

        this.AddDeploymentTargetsToFrameAncestors(deploymentTargets);
    }

    private string GetSchemeAndDomain(string url)
    {
        Uri uri = new Uri(url);
        return uri.Scheme + "://" + uri.Host;
    }

    private void AddDeploymentTargetsToFrameAncestors(List<string> deploymentTargets)
    {
        deploymentTargets.ForEach(dt => this.contentSecurityPolicy.AddFrameAncestorsDomain(dt));
    }

    private void AddScriptSourceDomains()
    {
        this.contentSecurityPolicy.AddScriptSrcDomain("'self'");
        this.contentSecurityPolicy.AddScriptSrcDomain("'unsafe-inline'");
        foreach (var url in this.contentSecurityPolicyConfiguration.PortalApp.ScriptDomains)
        {
            this.contentSecurityPolicy.AddScriptSrcDomain(url);
        }
    }

    private void AddDefaultSourceDomains()
    {
        this.contentSecurityPolicy.AddDefaultSrcDomain("'self'");
        this.contentSecurityPolicy.AddDefaultSrcDomain("data:");
        foreach (var url in this.contentSecurityPolicyConfiguration.PortalApp.DefaultDomains)
        {
            this.contentSecurityPolicy.AddDefaultSrcDomain(url);
        }
    }
}
