// <copyright file="EntityFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.File
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Model for creating an instance of <see cref="EntityFileProviderConfigModel"/>.
    /// </summary>
    public class EntityFileProviderConfigModel : IBuilder<IProvider<Data<FileInfo>>>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the name of the file attached in the entity.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> FileName { get; set; }

        /// <summary>
        /// Gets or sets the entity object where the file is attached.
        /// </summary>
        public IBuilder<BaseEntityProvider> Entity { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? OutputFileName { get; set; }

        /// <inheritdoc/>
        public IProvider<Data<FileInfo>> Build(IServiceProvider dependencyProvider)
        {
            var fileContentRepository = dependencyProvider.GetRequiredService<IFileContentRepository>();
            return new EntityFileProvider(
                this.OutputFileName?.Build(dependencyProvider),
                this.FileName.Build(dependencyProvider),
                this.Entity.Build(dependencyProvider),
                fileContentRepository);
        }
    }
}
