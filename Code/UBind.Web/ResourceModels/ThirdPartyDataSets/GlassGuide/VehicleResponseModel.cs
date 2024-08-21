// <copyright file="VehicleResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

public class VehicleResponseModel
{
    public IReadOnlyList<Vehicle>? Vehicles { get; set; }
}
