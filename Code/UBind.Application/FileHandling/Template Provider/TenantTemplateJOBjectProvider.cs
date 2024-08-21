// <copyright file="TenantTemplateJOBjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.Template_Provider
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Services;
    using UBind.Domain;

    /// <summary>
    /// Quote template JSON object provider.
    /// </summary>
    public class TenantTemplateJOBjectProvider : IJObjectProvider
    {
        private ITenantService tenantService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantTemplateJOBjectProvider"/> class.
        /// </summary>
        /// <param name="tenantService">The tenant service.</param>
        public TenantTemplateJOBjectProvider(ITenantService tenantService)
        {
            this.tenantService = tenantService;
        }

        /// <inheritdoc/>
        public JObject JsonObject { get; private set; } = new JObject();

        /// <inheritdoc/>
        public Task CreateJsonObject(ApplicationEvent applicationEvent)
        {
            dynamic jsonObject = new JObject();

            var tenant = this.tenantService.GetTenant(applicationEvent.Aggregate.TenantId);

            jsonObject.Id = tenant.Id;
            jsonObject.Name = tenant.Details.Name;
            jsonObject.Alias = tenant.Details.Alias;

            IJsonObjectParser parser = new GenericJObjectParser("Tenant", jsonObject);
            if (parser.JsonObject != null)
            {
                this.JsonObject = parser.JsonObject;
            }

            return Task.CompletedTask;
        }
    }
}
