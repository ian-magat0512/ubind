// <copyright file="VehicleDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ThirdPartyDataSets.RedBook
{
    public class VehicleDetails
    {
        public string VehicleKey { get; set; }

        public string VehicleDescription { get; set; }

        public string MakeCode { get; set; }

        public string MakeDescription { get; set; }

        public string FamilyCode { get; set; }

        public string FamilyDescription { get; set; }

        public string VehicleTypeCode { get; set; }

        public int YearGroup { get; set; }

        public int MonthGroup { get; set; }

        public int SequenceNumber { get; set; }

        public bool CurrentRelease { get; set; }

        public bool LimitedEdition { get; set; }

        public string SeriesPublic { get; set; }

        public string SeriesModelYear { get; set; }

        public string BadgeDescription { get; set; }

        public string BadgeSecondaryDescription { get; set; }

        public string BodyStyleDescription { get; set; }

        public string BodyConfigurationDescription { get; set; }

        public string WheelBaseConfig { get; set; }

        public string Roofline { get; set; }

        public string ExtraIdentification { get; set; }

        public string DriveDescription { get; set; }

        public string GearTypeDescription { get; set; }

        public string GearLocationDescription { get; set; }

        public int GearNum { get; set; }

        public int DoorNum { get; set; }

        public int EngineSize { get; set; }

        public string EngineDescription { get; set; }

        public int Cylinders { get; set; }

        public string FuelTypeDescription { get; set; }

        public string InductionDescription { get; set; }

        public string OptionCategory { get; set; }

        public int TradeMin { get; set; }

        public int TradeMax { get; set; }

        public int PrivateMin { get; set; }

        public int PrivateMax { get; set; }

        public string VFactsClass { get; set; }

        public string VFactsSegment { get; set; }

        public string VFactsPrice { get; set; }

        public string HighPoweredVehicle { get; set; }

        public bool IsPPlateApproved { get; set; }
    }
}
