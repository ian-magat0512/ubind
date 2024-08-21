// <copyright file="GetProductFileContentsByFileNameQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.AssetFile
{
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    public class GetProductFileContentsByFileNameQuery : IQuery<byte[]>
    {
        public GetProductFileContentsByFileNameQuery(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType,
            FileVisibility visibility,
            string fileName)
        {
            this.ReleaseContext = releaseContext;
            this.WebformAppType = webFormAppType;
            this.Visibility = visibility;
            this.FileName = fileName;
        }

        public ReleaseContext ReleaseContext { get; set; }

        public WebFormAppType WebformAppType { get; }

        public FileVisibility Visibility { get; }

        public string FileName { get; }
    }
}
