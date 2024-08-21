// <copyright file="StaticFileProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Infrastructure
{
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This is a provider for our static file which is to override
    /// getting of file information on file provider. We need to ignore the index.html for the static file
    /// so that it will redirect to the app controller instead. To config all the stuff need on our webFormApp.
    /// </summary>
    public class StaticFileProvider : IFileProvider
    {
        private readonly PhysicalFileProvider innerProvider;

        public StaticFileProvider(PhysicalFileProvider physicalFileProvider)
        {
            this.innerProvider = physicalFileProvider;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return this.innerProvider.GetDirectoryContents(subpath);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fileInfo = this.innerProvider.GetFileInfo(subpath);

            if (fileInfo.Exists && subpath.EqualsIgnoreCase("/index.html"))
            {
                fileInfo = this.innerProvider.GetFileInfo("/");
            }

            return fileInfo;
        }

        public IChangeToken Watch(string filter)
        {
            return this.innerProvider.Watch(filter);
        }
    }
}
