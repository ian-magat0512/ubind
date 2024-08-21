// <copyright file="PathLookupDateTimeProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime;

using NodaTime;
using UBind.Application.Automation.PathLookup;

/// <summary>
/// Represents the path lookup datetime configuration model defined by a object path lookup provider.
/// </summary>
public class PathLookupDateTimeProviderConfigModel : PathLookupValueTypeConfigModel<PathLookupDateTimeProvider, IProvider<Data<Instant>>>
{
}
