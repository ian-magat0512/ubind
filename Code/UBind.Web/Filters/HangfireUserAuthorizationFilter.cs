// <copyright file="HangfireUserAuthorizationFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.Filters
{
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Hangfire.Dashboard;
    using UBind.Application.Authorisation;
    using UBind.Domain;

    public class HangfireUserAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IServiceProvider serviceProvider;

        public HangfireUserAuthorizationFilter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public bool Authorize([NotNull] DashboardContext context)
        {
            this.ProcessRedirectAction(context).Wait();

            // This should always be true in order for the context to be redirected to whatever page direction it is to be set to.
            return true;
        }

        /// <summary>
        /// Processes the redirect action to the portal login page if the user is not logged in and has no access to the Hangfire Dashboard.
        /// </summary>
        /// <param name="context">DashboardContext object from Hangfire</param>
        private async Task ProcessRedirectAction(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            IRequestCookieCollection? cookies = httpContext.Request.Cookies;

            if (cookies != null && !cookies.ContainsKey(WebPortal.HangfireDashboardCookie))
            {
                this.SetRedirect(httpContext);

                // Perform a quick exit to save resource.
                return;
            }

            string token = cookies != null ? (cookies[WebPortal.HangfireDashboardCookie] ?? string.Empty) : string.Empty;

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? securityToken = handler.ReadToken(token) as JwtSecurityToken;

            if (securityToken != null)
            {
                var performingUser = new ClaimsPrincipal(new ClaimsIdentity(securityToken.Claims));
                using (var scope = this.serviceProvider.CreateScope())
                {
                    var authService = scope.ServiceProvider.GetService(typeof(IAuthorisationService)) as IAuthorisationService;
                    var hasPermission = authService != null && await authService.DoesUserHaveAccessToHangfireDashboard(performingUser);

                    if (!hasPermission)
                    {
                        this.SetRedirect(httpContext);
                    }
                }
            }
            else
            {
                this.SetRedirect(httpContext);
            }
        }

        /// <summary>
        /// Sets the redirect context to the portal login page in the event the current user does not have the proper permissions to access the Hangfire Dashboard.
        /// </summary>
        /// <param name="httpContext">HttpContext object from the Hangfire DashboardContext</param>
        private void SetRedirect(HttpContext httpContext)
        {
            // Get the original url requested by the user (Path only - host or domain excluded)
            HttpRequest request = httpContext.Request;
            string pathBase = request.PathBase.HasValue ? request.PathBase.ToString() : string.Empty;
            string path = request.Path.HasValue ? request.Path.ToString() : string.Empty;
            string queryString = request.QueryString.HasValue ? request.QueryString.Value ?? string.Empty : string.Empty;
            queryString = queryString.StartsWith('?') ? queryString.Substring(1, queryString.Length - 1) : queryString;
            string redirectUrl = $"{pathBase}{path}{(queryString.Length > 0 ? "?" : string.Empty)}{queryString}";

            // Convert it to base64 to avoid being malformed once receive by the client
            string base64Url = Convert.ToBase64String(Encoding.UTF8.GetBytes(redirectUrl));
            var queryParameters = queryString.Split("&")
                                  .Where(i => i.Contains('='))
                                  .Select(param =>
                                  new
                                  {
                                      Key = param.Split("=")[0],
                                      Value = param.Split("=")[1],
                                  });

            var isRedirectedParameter = queryParameters?
                                       .Where(param => param.Key == "isRedirected")
                                       .Select(param => param).FirstOrDefault();
            var isRedirected = bool.Parse(isRedirectedParameter?.Value ?? "false");

            httpContext.Response.Redirect($"/portal/ubind/path/login?redirectLocation={base64Url}{(isRedirected ? "&isRedirected=true" : string.Empty)}");
        }
    }
}