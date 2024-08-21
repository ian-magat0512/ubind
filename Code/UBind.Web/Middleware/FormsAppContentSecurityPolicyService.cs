// <copyright file="FormsAppContentSecurityPolicyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware;

using Microsoft.AspNetCore.Http;
using UBind.Application.Authorisation;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Product;
using UBind.Web.Configuration;
using UBind.Web.Helpers;

public class FormsAppContentSecurityPolicyService : IContentSecurityPolicyService
{
    private readonly IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration;
    private ContentSecurityPolicyBuilder contentSecurityPolicy;

    public FormsAppContentSecurityPolicyService(IContentSecurityPolicyConfiguration contentSecurityPolicyConfiguration)
    {
        this.contentSecurityPolicyConfiguration = contentSecurityPolicyConfiguration ?? throw new ArgumentNullException();
        this.contentSecurityPolicy = new ContentSecurityPolicyBuilder();
    }

    /// <summary>
    /// Get the Policy Header Value
    /// NOTE: This method is being called from a middleware as callback function (state)
    /// Do not throw exception inside the middleware callback function
    /// It will disrupt the request pipeline and throw a unhandled exception error
    /// </summary>
    /// <param name="httpContext">instance of HttpContext</param>
    /// <param name="cachingResolver">instance implementor of ICachingResolver</param>
    /// <param name="authorisationService">instance implementor of IAuthorisationService</param>
    /// <returns>PolicyHeaderValue if Valid Tenant, Product, Environment was found</returns>
    public async Task<string?> GetPolicyHeaderValue(HttpContext httpContext, ICachingResolver cachingResolver, IAuthorisationService authorisationService)
    {
        var product = await this.GetProduct(httpContext, cachingResolver);
        if (product == null)
        {
            return string.Empty;
        }
        var environment = this.GetEnvironment(httpContext);
        var settings = product.Details.DeploymentSetting;
        return this.IsFeatureEnabled(settings, environment)
            ? this.GetContentSecurityPolicyHeaderValue(product, environment)
            : string.Empty;
    }

    private async Task<Product?> GetProduct(HttpContext httpContext, ICachingResolver cachingResolver)
    {
        var tenantAlias = httpContext.Request.Query["tenant"].FirstOrDefault();
        var productAlias = httpContext.Request.Query["product"].FirstOrDefault();
        if (string.IsNullOrEmpty(tenantAlias)
            || string.IsNullOrEmpty(productAlias))
        {
            return null;
        }

        var tenantId = new GuidOrAlias(tenantAlias);
        var tenant = await cachingResolver.GetTenantOrNull(tenantId);
        if (tenant == null)
        {
            return null;
        }

        var productId = new GuidOrAlias(productAlias);
        var product = await cachingResolver.GetProductOrNull(tenant.Id, productId);

        return product;
    }

    private DeploymentEnvironment GetEnvironment(HttpContext httpContext)
    {
        var environmentAlias = httpContext.Request.Query["environment"].FirstOrDefault() ?? string.Empty;
        var environment = environmentAlias?.ToEnumOrNull<DeploymentEnvironment>() ?? DeploymentEnvironment.None;
        return environment;
    }

    private bool IsFeatureEnabled(ProductDeploymentSetting settings, DeploymentEnvironment environment)
    {
        if (settings == null)
        {
            return false;
        }

        switch (environment)
        {
            case DeploymentEnvironment.Development:
                if (settings.DevelopmentIsActive)
                {
                    return true;
                }
                break;
            case DeploymentEnvironment.Staging:
                if (settings.StagingIsActive)
                {
                    return true;
                }
                break;
            case DeploymentEnvironment.Production:
                if (settings.ProductionIsActive)
                {
                    return true;
                }
                break;
        }

        return false;
    }

    private string GetContentSecurityPolicyHeaderValue(Product product, DeploymentEnvironment environment)
    {
        this.AddFrameAncestorDomains(product, environment);
        this.AddScriptSourceDomains();
        this.AddStyleSourceDomains();
        this.AddDefaultSourceDomains();
        return this.contentSecurityPolicy.GetPolicyString();
    }

    private void AddFrameAncestorDomains(Product product, DeploymentEnvironment environment)
    {
        bool isDeploymentEnvironmentActive;
        this.AddDeploymentTargetsToFrameAncestors(product, environment, out isDeploymentEnvironmentActive);

        if (isDeploymentEnvironmentActive)
        {
            this.contentSecurityPolicy.AddFrameAncestorsDomain("'self'");
            foreach (var url in this.contentSecurityPolicyConfiguration.FormsApp.FrameAncestorDomains)
            {
                this.contentSecurityPolicy.AddFrameAncestorsDomain(url);
            }
        }
    }

    private void AddScriptSourceDomains()
    {
        this.contentSecurityPolicy.AddScriptSrcDomain("'self'");

        // TODO: Once we have upgraded to Angular 16 or later, remove the 'unsafe-eval' and we should use the nonce or CSP_NONCE
        // attribute in our front end to allow inline scripts. Please see https://angular.io/guide/security#content-security-policy
        // and https://github.com/angular/angular/blob/main/aio/content/guide/security.md#content-security-policy for more details
        // on this. As for now, we can't use the nonce attribute because our current angular version doesn't support it which is why
        // we'll have to use this temporary workaround to allow us to use inline scripts and so our .
        this.contentSecurityPolicy.AddScriptSrcDomain("'unsafe-eval'");

        foreach (var url in this.contentSecurityPolicyConfiguration.FormsApp.ScriptDomains)
        {
            this.contentSecurityPolicy.AddScriptSrcDomain(url);
        }
    }

    private void AddStyleSourceDomains()
    {
        this.contentSecurityPolicy.AddStyleSrcDomain("'self'");
        this.contentSecurityPolicy.AddStyleSrcDomain("'unsafe-inline'");
        this.contentSecurityPolicy.AddStyleSrcDomain("*.ubind.com.au");
        this.contentSecurityPolicy.AddStyleSrcDomain("*.ubind.io");
        foreach (var url in this.contentSecurityPolicyConfiguration.FormsApp.StyleDomains)
        {
            this.contentSecurityPolicy.AddStyleSrcDomain(url);
        }
    }

    private void AddDefaultSourceDomains()
    {
        this.contentSecurityPolicy.AddDefaultSrcDomain("'self'");
        this.contentSecurityPolicy.AddDefaultSrcDomain("data:");
        foreach (var url in this.contentSecurityPolicyConfiguration.FormsApp.DefaultDomains)
        {
            this.contentSecurityPolicy.AddDefaultSrcDomain(url);
        }
    }

    private void AddDeploymentTargetsToFrameAncestors(Product product, DeploymentEnvironment environment, out bool isDeploymentEnvironmentActive)
    {
        // set the default policy which is to allow the same domain only.
        isDeploymentEnvironmentActive = false;
        if (product.Details.DeploymentSetting == null)
        {
            return;
        }

        var setting = product.Details.DeploymentSetting;
        switch (environment)
        {
            case DeploymentEnvironment.Development:
                if (!setting.DevelopmentIsActive)
                {
                    // we won't set a CSP header at all
                    return;
                }

                isDeploymentEnvironmentActive = true;
                setting.Development.ForEach(domain => this.contentSecurityPolicy.AddFrameAncestorsDomain($"{domain}"));
                break;
            case DeploymentEnvironment.Staging:
                if (!setting.StagingIsActive)
                {
                    // we won't set a CSP header at all
                    return;
                }

                isDeploymentEnvironmentActive = true;
                setting.Staging.ForEach(domain => this.contentSecurityPolicy.AddFrameAncestorsDomain($"{domain}"));
                break;
            case DeploymentEnvironment.Production:
                if (!setting.ProductionIsActive)
                {
                    // we won't set a CSP header at all
                    return;
                }

                isDeploymentEnvironmentActive = true;
                setting.Production.ForEach(domain => this.contentSecurityPolicy.AddFrameAncestorsDomain($"{domain}"));
                break;
        }
    }
}
