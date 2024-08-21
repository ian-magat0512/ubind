// <copyright file="UbindApplicationDependencyInjection.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Reflection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// UBindApplicationDependencyInjection.
    /// </summary>
    public static class UBindApplicationDependencyInjection
    {
        /// <summary>
        /// UBind Application Injection.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        /// <returns>IServiceCollection.</returns>
        public static IServiceCollection AddUbindApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            return services;
        }
    }
}
