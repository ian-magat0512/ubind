// <copyright file="PostcodeValidatorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person.Fields
{
    using System.Globalization;

    /// <summary>
    /// The interface for validating post codes.
    /// </summary>
    public static class PostcodeValidatorFactory
    {
        public static IPostcodeValidator? GetPostcodeValidatorByCountryCode(string twoLetterCountryCode)
        {
            // Create a new RegionInfo instance for the United States using the two-letter country code
            RegionInfo regionInfo = new RegionInfo(twoLetterCountryCode);

            // Get the two-letter country code
            string countryCode = regionInfo.TwoLetterISORegionName;

            switch (countryCode)
            {
                case "AU":
                    return new AustralianPostcodeValidator();
                default:
                    return null;
            }
        }
    }
}
