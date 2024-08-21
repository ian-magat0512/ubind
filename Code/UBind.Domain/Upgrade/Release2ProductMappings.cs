// <copyright file="Release2ProductMappings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Upgrade
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides mappings to use when updating database and One Drive for release 2.0.
    /// </summary>
    public static class Release2ProductMappings
    {
        /// <summary>
        /// Provide mappings to use when updating database and One Drive for release 2.0.
        /// </summary>
        public static readonly IEnumerable<ProductMigrationMapping> ProductMappings = new List<ProductMigrationMapping>
        {
            new ProductMigrationMapping("Allianz-RoadsideAssistance-Demo", "Demos", "demos", "Allianz Roadside Assistance", "allianz-roadside-assistance"),
            new ProductMigrationMapping("Demo-Investment", "Demos", "demos", "Investment", "investment"),
            new ProductMigrationMapping("Demo-Landlords", "Demos", "demos", "Landlords", "landlords"),
            new ProductMigrationMapping("Demo-Trades", "Demos", "demos", "Trades", "trades"),
            new ProductMigrationMapping("DepositAssure-Concierge", "Deposit Assure", "deposit-assure", "Concierge", "concierge"),
            new ProductMigrationMapping("DepositAssure-Consumer", "Deposit Assure", "deposit-assure", "Customer", "customer"),
            new ProductMigrationMapping("FiGi-LifeStylePlus", "FiGi", "figi", "Lifestyle Plus", "lifestyle-plus"),
            new ProductMigrationMapping("GriffithsGoodall", "Griffiths Goodall", "griffiths-goodall", "Tax Audit", "tax-audit"),
            new ProductMigrationMapping("HearInsure", "HearInsure", "hearinsure", "Heading Aids", "hearing-aids"),
            new ProductMigrationMapping("Hyperion", "Hyperion", "hyperion", "Online Application", "online-application"),
            new ProductMigrationMapping("Insure247", "Insure 247", "insure247", "Trades", "trades"),
            new ProductMigrationMapping("Insure247-Farm", "Insure 247", "insure247", "Farm", "farm"),
            new ProductMigrationMapping("Latitude-Claim-Demo", "Demos", "demos", "Latitude Claim", "latitude-claim"),
            new ProductMigrationMapping("LatitudeFinancial", "Demos", "demos", "Latitude Financial", "latitude-financial"),
            new ProductMigrationMapping("Latitude-MotorLoan-Demo", "Demos", "demos", "Latitude Motor Loan", "latitude-motor-loan"),
            new ProductMigrationMapping("Latitude-Travel-Demo", "Demos", "demos", "Latitude Travel", "latitude-travel"),
            new ProductMigrationMapping("MilneAlexander", "Milne Alexander", "milne-alexander", "Commercial Motor", "commercial-motor"),
            new ProductMigrationMapping("Motorpac", "Lease & Asset Finance", "lease-asset-finance", "Motorpac", "motorpac"),
            new ProductMigrationMapping("Pets", "MGA", "mga", "Pets", "pets"),
            new ProductMigrationMapping("PromoInABox", "Insure my Promo", "insure-my-promo", "Promo In a Box", "promoinabox"),
            new ProductMigrationMapping("PSC-Direct", "PSC", "psc", "Trades", "trades"),
            new ProductMigrationMapping("PSC-PropertyClub", "Australian Reliance", "australian-reliance", "Property Club", "property-club"),
            new ProductMigrationMapping("Realestate-Demo", "Demos", "demos", "Real Estate", "real-estate"),
            new ProductMigrationMapping("Sherwood-Travel", "Demos", "demos", "Sherwood Travel", "sherwood-travel"),
            new ProductMigrationMapping("Strategic-Pilot", "Strategic", "strategic", "Pilots", "pilots"),
            new ProductMigrationMapping("Thunder-Console", "Thunder", "thunder", "Console", "console"),
            new ProductMigrationMapping("TISCyber", "TIS", "tis", "Cyber", "cyber"),
            new ProductMigrationMapping("Websters", "Demos", "demos", "Websters", "websters"),
        };
    }
}
