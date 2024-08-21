// <copyright file="PathLookupFileProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.File;

using UBind.Application.Automation.PathLookup;

/// <summary>
/// Converter for deserializing path lookup file object from json.
/// </summary>
/// <remarks>
/// Convert objects with JSON pointer value indicating a location of the file reference within the automation data.
/// </remarks>
public class PathLookupFileProviderConfigModelConverter : PathLookupValueTypeProviderConfigModelConverter<PathLookupFileProviderConfigModel>
{
    public PathLookupFileProviderConfigModelConverter()
        : base("objectPathLookupFile")
    {
    }
}
