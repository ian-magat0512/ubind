// <copyright file="PathLookupFileProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File;

using System;
using Microsoft.Extensions.DependencyInjection;
using UBind.Application.Automation.PathLookup;
using UBind.Domain.Repositories;

/// <summary>
/// Model for creating an instance of <see cref="PathLookupFileProvider"/>.
/// </summary>
public class PathLookupFileProviderConfigModel : PathLookupValueTypeConfigModel<PathLookupFileProvider, IPathLookupFileProvider>
{
    public override IPathLookupFileProvider Build(IServiceProvider dependencyProvider)
    {
        var fileContentRepository = dependencyProvider.GetService<IFileContentRepository>();

        return new PathLookupFileProvider(
            this.PathLookup?.Build(dependencyProvider),
            (IProvider<Data<FileInfo>>)this.ValueIfNotFound?.Build(dependencyProvider),
            fileContentRepository,
            this.RaiseErrorIfNotFound?.Build(dependencyProvider),
            this.RaiseErrorIfNull?.Build(dependencyProvider),
            (IProvider<Data<FileInfo>>)this.ValueIfNull?.Build(dependencyProvider),
            this.RaiseErrorIfTypeMismatch?.Build(dependencyProvider),
            (IProvider<Data<FileInfo>>)this.ValueIfTypeMismatch?.Build(dependencyProvider),
            (IProvider<Data<FileInfo>>)this.DefaultValue?.Build(dependencyProvider));
    }
}
