// <copyright file="AppFilesHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers;

/// <summary>
/// In development, the static files for the PortalApp, FormsApp and Injection are located in a
/// separate project alongside the main project. In production, they are located as a subdirectory
/// to the publish output folder.
/// </summary>
public static class AppFilesHelper
{
    /// <summary>
    /// Gets the location of the static files for the PortalApp.
    /// </summary>
    /// <param name="env">The web host environment, so we can know if it's development or not.</param>
    public static string GetPortalAppStaticFilesPath(IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            return Path.Combine(Directory.GetCurrentDirectory(), @"..\UBind.PortalApp\dist");
        }
        else
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "PortalApp");
        }
    }

    /// <summary>
    /// Gets the location of the static files for the FormsApp.
    /// </summary>
    /// <param name="env">The web host environment, so we can know if it's development or not.</param>
    public static string GetFormsAppStaticFilesPath(IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            return Path.Combine(Directory.GetCurrentDirectory(), @"..\UBind.FormsApp\dist");
        }
        else
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "FormsApp");
        }
    }

    /// <summary>
    /// Gets the location of the static files for the Injection app.
    /// </summary>
    /// <param name="env">The web host environment, so we can know if it's development or not.</param>
    public static string GetInjectionStaticFilesPath(IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            return Path.Combine(Directory.GetCurrentDirectory(), @"..\UBind.Injection\dist");
        }
        else
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "Injection");
        }
    }
}
