// <copyright file="HomeController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using UBind.Web.Filters;

    /// <summary>
    /// Home controller.
    /// </summary>
    [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
    public class HomeController : Controller
    {
        /// <summary>
        /// Serve home page.
        /// </summary>
        /// <returns>Home page.</returns>
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Serve error page.
        /// </summary>
        /// <returns>Error page.</returns>
        public IActionResult Error()
        {
            return this.View();
        }
    }
}
