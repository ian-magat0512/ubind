// <copyright file="IDataImport.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

using System.Data;

public interface IDataImport
{
    /// <summary>
    /// Imports from data component file to data table.
    /// </summary>
    /// <param name="segment">The data cegment (e.g. PVG, OCG, ...).</param>
    /// <param name="type">The data component type (e.g. N12, U12, MAK, ...).</param>
    /// <param name="canAddValues">Indicates whether adding values from missing rows is supported.</param>
    /// <param name="canValidateValues">Indicates whether cell value validation is supported.</param>
    /// <param name="canRecode">Indicates whether recode (change of glass code or delete) is supported.</param>
    Task<DataTable> Import(string segment, DataComponentType type, bool canAddValues = false, bool canValidateValues = false, bool canRecode = false);
}
