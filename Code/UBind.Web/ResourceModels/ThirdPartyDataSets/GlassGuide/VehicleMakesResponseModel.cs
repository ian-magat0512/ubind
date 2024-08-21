// <copyright file="VehicleMakesResponseModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Web.ResourceModels.ThirdPartyDataSets.GlassGuide;

using System.Collections.Generic;
using UBind.Domain.ThirdPartyDataSets.GlassGuide;

public class VehicleMakesResponseModel
{
    public IEnumerable<VehicleMake>? Makes { get; set; }
}