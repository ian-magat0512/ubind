// <copyright file="IContentSecurityPolicyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Middleware;

using UBind.Application.Authorisation;
using UBind.Domain;

public interface IContentSecurityPolicyService
{
    Task<string?> GetPolicyHeaderValue(HttpContext context, ICachingResolver cachingResolver, IAuthorisationService authorisationService);
}
