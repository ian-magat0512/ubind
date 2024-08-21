// <copyright file="VehicleFamilyResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

public class VehicleFamilyResponseModel
{
    public IEnumerable<VehicleFamily>? Families { get; set; }
}
