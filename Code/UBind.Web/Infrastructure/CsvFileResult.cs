// <copyright file="CsvFileResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Represents ActionResult that when executed it will
    /// create a csv file as the response.
    /// </summary>
    public class CsvFileResult : FileResult
    {
        private readonly string csvString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvFileResult"/> class.
        /// </summary>
        /// <param name="csvString">The csv string.</param>
        /// <param name="fileDownloadName">The download filename.</param>
        public CsvFileResult(string csvString, string fileDownloadName)
            : base("text/csv")
        {
            this.csvString = csvString;
            this.FileDownloadName = fileDownloadName;
        }

        /// <summary>
        /// This method is called by MVC to process the result of an action method.
        /// </summary>
        /// <param name="context">The action context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var fileDownloadName = new FileName(this.FileDownloadName);
            var bytes = Encoding.UTF8.GetBytes(this.csvString);
            context.HttpContext.Response.Headers.Add("Content-Disposition", new[] { "attachment; filename=" + fileDownloadName });
            await context.HttpContext.Response.Body.WriteAsync(bytes);
        }
    }
}
