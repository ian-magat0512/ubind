// <copyright file="FallbackMiddleware.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware;

using Microsoft.Extensions.FileProviders;
using UBind.Web.Helpers;

public class FallbackMiddleware
{
    private readonly RequestDelegate next;
    private readonly IWebHostEnvironment env;
    private readonly PhysicalFileProvider fileProvider;
    private readonly string portalDefaultFilename;
    private ReadOnlyMemory<byte> portalDefaultFileContent;

    public FallbackMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        this.next = next;
        this.env = env;

        // configure file provider and default file
        string appFilesPath = AppFilesHelper.GetPortalAppStaticFilesPath(env);
        this.fileProvider = new PhysicalFileProvider(appFilesPath);
        this.portalDefaultFilename = "index.html";

        if (!env.IsDevelopment())
        {
            var fileInfo = this.fileProvider.GetFileInfo(this.portalDefaultFilename);
            if (fileInfo.Exists)
            {
                // Read the file into memory
                using var stream = fileInfo.CreateReadStream();
                var fileContent = new byte[stream.Length];
                stream.Read(fileContent, 0, fileContent.Length);
                this.portalDefaultFileContent = new ReadOnlyMemory<byte>(fileContent);
            }
            else
            {
                throw new FileNotFoundException($"Could not find the default file: {this.portalDefaultFilename}");
            }
        }
    }

    public async Task Invoke(HttpContext context)
    {
        await this.next(context);

        if (context.Response.StatusCode == 404 && context.Request.Path.StartsWithSegments("/portal"))
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;  // reset the status code

            if (this.env.IsDevelopment())
            {
                // In development, read the file from disk each time
                var fileInfo = this.fileProvider.GetFileInfo(this.portalDefaultFilename);
                if (fileInfo.Exists)
                {
                    await context.Response.SendFileAsync(fileInfo);
                }
            }
            else
            {
                // In non-development, use the file content cached in memory
                await context.Response.Body.WriteAsync(this.portalDefaultFileContent);
            }
        }
    }
}
