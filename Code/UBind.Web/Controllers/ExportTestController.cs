// <copyright file="ExportTestController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using UBind.Application;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Controller for One Drive notifications.
    /// </summary>
    [Produces(ContentTypes.Json)]
    [Route("exporttest")]
    public class ExportTestController : Controller
    {
        private const string TestFolder = "TestExports";
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportTestController"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">Hostirng environment.</param>
        /// <param name="clock">You know what a clock is.</param>
        public ExportTestController(IWebHostEnvironment hostingEnvironment, IClock clock)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.clock = clock;
        }

        private string FolderPath => Path.Combine(this.hostingEnvironment.ContentRootPath, TestFolder);

        /// <summary>
        /// Handle test exports.
        /// </summary>
        /// <returns>An awaitable task.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ExportTest()
        {
            var now = this.clock.GetCurrentInstant();
            var zone = DateTimeZoneProviders.Bcl.GetSystemDefault();
            var zonedNow = now.InZone(zone);
            var utcNow = now.InUtc();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Local time: {zonedNow}");
            stringBuilder.AppendLine($"UTC time: {utcNow}");
            stringBuilder.AppendLine($"{this.Request.Method} {this.Request.GetEncodedUrl()}");
            foreach (var key in this.Request.Headers.Keys)
            {
                stringBuilder.AppendLine($"{key}: {this.Request.Headers[key]}");
            }

            var memoryStream = new MemoryStream();
            await this.Request.Body.CopyToAsync(memoryStream);
            var body = Encoding.ASCII.GetString(memoryStream.ToArray());
            stringBuilder.AppendLine(body);
            var log = stringBuilder.ToString();
            Directory.CreateDirectory(this.FolderPath);
            var fileName = now.ToUnixTimeTicks() + ".log";
            var filePath = Path.Combine(this.FolderPath, fileName);
            System.IO.File.WriteAllText(filePath, log);
            return this.NoContent();
        }

        /// <summary>
        /// Gets the logs of all exports.
        /// </summary>
        /// <returns>An action result containing the logs as json.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ExportTestLogModel>), StatusCodes.Status200OK)]
        public IActionResult GetAllExports()
        {
            var logs = Directory
                .EnumerateFiles(this.FolderPath)
                .Select(path => new ExportTestLogModel(
                    long.Parse(Path.GetFileNameWithoutExtension(path)),
                    System.IO.File.ReadAllText(path)));
            return this.Ok(logs);
        }
    }
}
