// <copyright file="ProductFilesController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.IO;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for product requests.
    /// </summary>
    [MustBeLoggedIn]
    [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
    [Produces("application/json")]
    [Route("~/api/{tenant}/{environment}/{product}/product")]
    public class ProductFilesController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IProductService productService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFilesController"/> class.
        /// </summary>
        /// <param name="productService">The product application service.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public ProductFilesController(
            IProductService productService,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.productService = productService;
        }

        /// <summary>
        /// Seeds files.
        /// </summary>
        /// <param name="tenant">The tenant ID or Alias.</param>
        /// <param name="environment">environment.</param>
        /// <param name="product">the product ID or Alias.</param>
        /// <param name="file">file.</param>
        /// <param name="folder">folder to place to files to.</param>
        /// <returns>OK.</returns>
        [HttpPost]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 5)]
        [DisableRequestSizeLimit]
        [ValidateModel]
        [Route("seed")]
        [MustHavePermission(Permission.ManageProducts)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async System.Threading.Tasks.Task<IActionResult> SeedAsync(
            string tenant,
            string environment,
            string product,
            [FromForm] IFormFile file,
            [FromQuery] string folder = null)
        {
            // upload file
            if (!(file == null || file.Length == 0))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var bytes = ms.ToArray();

                    var fileModel = new FileModel(file.FileName, bytes, file.ContentType);

                    var productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
                    await this.productService.SeedFilesAsync(productModel.TenantId, productModel.Id, environment, fileModel, folder);
                }
            }

            return this.Ok();
        }
    }
}
