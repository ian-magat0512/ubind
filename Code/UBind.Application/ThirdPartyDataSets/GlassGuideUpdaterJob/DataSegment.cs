// <copyright file="DataSegment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.GlassGuideUpdaterJob;

public class DataSegment
{
    /// <summary>
    /// Defines vehicle information based on data segment file.
    /// </summary>
    /// <param name="filepath">The full path of data segment file.</param>
    public DataSegment(string filepath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filepath);
        string dataCode = (fileName.Length > 3) ? fileName.Substring(0, 3) : string.Empty;
        this.DataCode = dataCode.ToUpper() switch
        {
            "PVG" => VehicleSegmentType.Pvg,
            "OCG" => VehicleSegmentType.Ocg,
            "CVG" => VehicleSegmentType.Cvg,
            "OLC" => VehicleSegmentType.Olc,
            _ => VehicleSegmentType.Unknown,
        };
        this.VehicleTypeCode = this.DataCode switch
        {
            VehicleSegmentType.Pvg => "PV",
            VehicleSegmentType.Ocg => "PV",
            VehicleSegmentType.Cvg => "CV",
            VehicleSegmentType.Olc => "CV",
            _ => string.Empty,
        };
        this.VehicleTypeDescription = this.DataCode switch
        {
            VehicleSegmentType.Pvg => "Passenger Vehicle",
            VehicleSegmentType.Ocg => "Passenger Vehicle",
            VehicleSegmentType.Cvg => "Light Commercial Vehicle",
            VehicleSegmentType.Olc => "Light Commercial Vehicle",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Glass's Guide vehicle data code, e.g. PVG
    /// </summary>
    public VehicleSegmentType DataCode { get; private set; }

    /// <summary>
    /// Glass's Guide vehicle type code, e.g. PV
    /// </summary>
    public string VehicleTypeCode { get; private set; }

    /// <summary>
    /// Glass's Guide vehicle type description, e.g. Passenger Vehicle
    /// </summary>
    public string VehicleTypeDescription { get; private set; }
}
