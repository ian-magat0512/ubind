﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UBind.Persistence.ThirdPartyDataSets {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class RedBookRepositoryScripts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RedBookRepositoryScripts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UBind.Persistence.ThirdPartyDataSets.RedBookRepositoryScripts", typeof(RedBookRepositoryScripts).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE @Schema NVARCHAR(MAX);
        ///DECLARE @TableIndex NVARCHAR(MAX);
        ///DECLARE @CreateIndex NVARCHAR(MAX);
        ///
        ///SET @Schema = &apos;{0}&apos;
        ///SET @TableIndex = &apos;{1}&apos;
        ///
        ///IF @TableIndex != &apos;&apos;
        ///BEGIN
        ///	SET @TableIndex = &apos;_&apos; + @TableIndex
        ///END
        ///
        ///IF NOT EXISTS (
        ///		SELECT *
        ///		FROM sys.indexes
        ///		WHERE name = &apos;Index1_VEVehicle&apos; + @TableIndex
        ///			AND object_id = OBJECT_ID(&apos;&apos; + @Schema + &apos;.VEVehicle&apos; + @TableIndex + &apos;&apos;)
        ///		)
        ///BEGIN
        ///
        ///SET @CreateIndex = N&apos;
        ///	CREATE NONCLUSTERED INDEX [Index1_VEVehicle&apos; + @TableIndex + &apos;] ON [&apos; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Add_Index_And_Fk_Constraints {
            get {
                return ResourceManager.GetString("Add_Index_And_Fk_Constraints", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DECLARE @Schema NVARCHAR(MAX);
        ///DECLARE @TableIndex NVARCHAR(MAX);
        ///DECLARE @CreateVEFamilyTable NVARCHAR(MAX);
        ///DECLARE @CreateVEMakeTable NVARCHAR(MAX);
        ///DECLARE @CreateVEVehicleTable NVARCHAR(MAX);
        ///DECLARE @CreateVEYearTable NVARCHAR(MAX);
        ///
        ///SET @Schema = &apos;{0}&apos;
        ///SET @TableIndex = &apos;{1}&apos;
        ///IF @TableIndex != &apos;&apos;
        ///BEGIN
        ///	SET @TableIndex = &apos;_&apos; + @TableIndex
        ///END
        ///SET @CreateVEFamilyTable = N&apos;
        ///	CREATE TABLE [&apos; + @Schema + &apos;].[VEFamily&apos; + @TableIndex + &apos;](
        ///		[MakeCode] [varchar](50) NULL,
        ///		[FamilyCode] [va [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CreateTable {
            get {
                return ResourceManager.GetString("CreateTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to WITH Cte_Vehicles AS (
        ///SELECT
        ///	Vehicles.BadgeDescription,
        ///	Vehicles.BodyStyleDescription,
        ///	Vehicles.GearTypeDescription
        ///FROM
        ///    [RedBook].[VEVehicle_View] Vehicles
        ///WHERE
        ///	-- Required Parameters
        ///    Vehicles.MakeCode = @MakeCode
        ///    AND Vehicles.FamilyCode = @FamilyCode
        ///    AND Vehicles.YearGroup = @Year
        ///)
        ///
        ///SELECT 
        ///	(SELECT  COUNT(DISTINCT BadgeDescription) FROM Cte_Vehicles 
        ///	WHERE BadgeDescription = @Badge) AS CountBadge,
        ///
        ///	(SELECT  COUNT(DISTINCT BodyStyleDescription) FROM Cte_Vehicles  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string RedBook_Dml_GetVehicleBadgeGearAndBodyCount {
            get {
                return ResourceManager.GetString("RedBook_Dml_GetVehicleBadgeGearAndBodyCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	DISTINCT Vehicles.BadgeDescription as Description,
        ///	ISNULL(Vehicles.BadgeSecondaryDescription, &apos;&apos;) as SecondaryDescription,
        ///	Vehicles.MakeCode,
        ///	Vehicles.FamilyCode,
        ///	Vehicles.VehicleTypeCode
        ///FROM
        ///    [RedBook].[VEVehicle_View] Vehicles
        ///WHERE
        ///
        ///	-- Required Parameters
        ///    Vehicles.MakeCode = @MakeCode
        ///    AND Vehicles.FamilyCode = @FamilyCode
        ///    AND Vehicles.YearGroup = @Year
        ///
        ///	-- Optional Parameters
        ///	AND ( 
        ///			(@BodyStyle = &apos;&apos; AND Vehicles.VehicleKey IS NOT NULL) 
        ///			OR (@BodyStyl [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Dml_GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType {
            get {
                return ResourceManager.GetString("Dml_GetVehicleBadgesByMakeCodeFamilyCodeYearBodyStyleAndGearType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///    DISTINCT Vehicles.BodyStyleDescription as Description
        ///FROM
        ///    [RedBook].[VEVehicle_View] Vehicles
        ///WHERE
        ///
        ///	-- Required Parameters
        ///    Vehicles.MakeCode = @MakeCode
        ///    AND Vehicles.FamilyCode = @FamilyCode
        ///    AND Vehicles.YearGroup = @Year
        ///
        ///	-- Optional Parameters
        ///	AND ( 
        ///			(@Badge = &apos;&apos; AND Vehicles.VehicleKey IS NOT NULL) 
        ///			OR (@Badge &lt;&gt; &apos;&apos; AND  Vehicles.BadgeDescription = @Badge ) 
        ///		)
        ///
        ///	AND (   
        ///			(@GearType = &apos;&apos; AND Vehicles.VehicleKey IS NOT NULL)
        ///			OR (@GearType &lt;&gt;  [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Dml_GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType {
            get {
                return ResourceManager.GetString("Dml_GetVehicleBodyStyleByMakeCodeFamilyCodeYearBadgeAndGearType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///    Vehicle.VehicleKey,
        ///    Vehicle.Description AS vehicleDescription,
        ///    Vehicle.MakeCode,
        ///    Make.Description AS makeDescription,
        ///    Vehicle.FamilyCode,
        ///    Make.Description AS familyDescription,
        ///    Vehicle.VehicleTypeCode,
        ///    Vehicle.YearGroup,
        ///    Vehicle.MonthGroup,
        ///    Vehicle.SequenceNum AS sequenceNumber,
        ///    CASE
        ///        Vehicle.CurrentRelease
        ///        WHEN &apos;F&apos; THEN CAST(0 AS BIT)
        ///        ELSE CAST(1 AS BIT)
        ///    END AS CurrentRelease,
        ///    CASE
        ///        Vehicle.LimitedEdit [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Dml_GetVehicleByVehicleKey {
            get {
                return ResourceManager.GetString("Dml_GetVehicleByVehicleKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT [MakeCode]
        ///	,[FamilyCode]
        ///	,[VehicleTypeCode]
        ///	,[Description]
        ///	,[StartYear]
        ///	,[LatestYear]
        ///FROM [RedBook].[VEFamily_View]
        ///WHERE [MakeCode] = @MakeCode
        ///ORDER BY Description.
        /// </summary>
        public static string Dml_GetVehicleFamiliesByMakeCode {
            get {
                return ResourceManager.GetString("Dml_GetVehicleFamiliesByMakeCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT [MakeCode]
        ///	,[FamilyCode]
        ///	,[VehicleTypeCode]
        ///	,[Description]
        ///	,[StartYear]
        ///	,[LatestYear]
        ///FROM [RedBook].[VEFamily_View]
        ///WHERE [MakeCode] = @MakeCode
        ///	AND @Year BETWEEN StartYear
        ///		AND LatestYear
        ///ORDER BY Description.
        /// </summary>
        public static string Dml_GetVehicleFamiliesByMakeCodeAndYear {
            get {
                return ResourceManager.GetString("Dml_GetVehicleFamiliesByMakeCodeAndYear", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEFamily_View] 
        ///     WHERE [MakeCode] = @MakeCode) AS CountFamily,
        ///
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEVehicle_View] 
        ///     WHERE [MakeCode] = @MakeCode AND [FamilyCode] = @FamilyCode) AS CountVehicle,
        ///
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEFamily_View] 
        ///     WHERE @YearGroup BETWEEN StartYear AND LatestYear) AS CountYearGroup
        ///.
        /// </summary>
        public static string RedBook_Dml_GetVehicleFamilyAndYearGroupCount {
            get {
                return ResourceManager.GetString("RedBook_Dml_GetVehicleFamilyAndYearGroupCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///   DISTINCT Vehicles.GearTypeDescription as Description
        ///FROM
        ///    [RedBook].[VEVehicle_View] AS Vehicles
        ///WHERE
        ///	-- Required Parameters
        ///    Vehicles.MakeCode = @MakeCode
        ///    AND Vehicles.FamilyCode = @FamilyCode
        ///    AND Vehicles.YearGroup = @Year
        ///
        ///	-- Optional Parameters
        ///	AND ( 
        ///			(@Badge = &apos;&apos; AND Vehicles.VehicleKey IS NOT NULL) 
        ///			OR (@Badge &lt;&gt; &apos;&apos; AND  Vehicles.BadgeDescription = @Badge ) 
        ///		)
        ///
        ///	AND (   
        ///			(@BodyStyle = &apos;&apos; AND Vehicles.VehicleKey IS NOT NULL) 
        ///			OR (@BodyStyle &lt; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Dml_GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle {
            get {
                return ResourceManager.GetString("Dml_GetVehicleGearTypeByMakeCodeFamilyCodeYearBadgeAndBodyStyle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///    DISTINCT Vehicles.VehicleKey,
        ///    Vehicles.Description AS VehicleDescription,
        ///    Vehicles.MakeCode,
        ///    VehicleMake.Description AS MakeDescription,
        ///    Vehicles.FamilyCode,
        ///    VehicleFamily.Description AS FamilyDescription,
        ///    Vehicles.YearGroup,
        ///    Vehicles.MonthGroup
        ///FROM
        ///    [RedBook].[VEVehicle_View] AS Vehicles
        ///    INNER JOIN [RedBook].[VEMake_View] AS VehicleMake ON Vehicles.MakeCode = VehicleMake.MakeCode
        ///    INNER JOIN [RedBook].[VEFamily_View] AS VehicleFamily ON Vehicles [rest of string was truncated]&quot;;.
        /// </summary>
        public static string Dml_GetVehicleList {
            get {
                return ResourceManager.GetString("Dml_GetVehicleList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT MakeCode
        ///	,Description
        ///	,StartYear
        ///	,LatestYear
        ///FROM  RedBook.VEMake_View
        ///ORDER BY Description
        ///.
        /// </summary>
        public static string Dml_GetVehicleMakes {
            get {
                return ResourceManager.GetString("Dml_GetVehicleMakes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT MakeCode
        ///	,Description
        ///	,StartYear
        ///	,LatestYear
        ///FROM RedBook.VEMake_View
        ///WHERE @Year BETWEEN StartYear
        ///		AND LatestYear
        ///ORDER BY Description
        ///.
        /// </summary>
        public static string Dml_GetVehicleMakesByYear {
            get {
                return ResourceManager.GetString("Dml_GetVehicleMakesByYear", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT YearGroup
        ///FROM [RedBook].[VEYear_View]
        ///WHERE [MakeCode] = @MakeCode
        ///GROUP BY YearGroup
        ///ORDER BY YearGroup DESC.
        /// </summary>
        public static string Dml_GetVehicleYearsByMakeCode {
            get {
                return ResourceManager.GetString("Dml_GetVehicleYearsByMakeCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT YearGroup
        ///FROM [RedBook].[VEYear_View]
        ///WHERE [MakeCode] = @MakeCode and
        ///       FamilyCode = @FamilyCode
        ///GROUP BY YearGroup
        ///ORDER BY YearGroup DESC
        ///.
        /// </summary>
        public static string Dml_GetVehicleYearsByMakeCodeAndFamilyCode {
            get {
                return ResourceManager.GetString("Dml_GetVehicleYearsByMakeCodeAndFamilyCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEFamily_View] 
        ///     WHERE [MakeCode] = @MakeCode) AS CountFamily,
        ///
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEVehicle_View] 
        ///     WHERE [MakeCode] = @MakeCode AND [FamilyCode] = @FamilyCode) AS CountVehicle,
        ///
        ///    (SELECT COUNT(1) 
        ///     FROM [RedBook].[VEFamily_View] 
        ///     WHERE @YearGroup BETWEEN StartYear AND LatestYear) AS CountYearGroup.
        /// </summary>
        public static string Dml_IsVehicleFamilyMakeCodeExists {
            get {
                return ResourceManager.GetString("Dml_IsVehicleFamilyMakeCodeExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  count(1) FROM [RedBook].[VEFamily_View] WHERE @YearGroup BETWEEN StartYear AND LatestYear
        ///.
        /// </summary>
        public static string Dml_IsVehicleFamilyYearGroupExists {
            get {
                return ResourceManager.GetString("Dml_IsVehicleFamilyYearGroupExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  count(1) FROM [RedBook].[VEMake_View] WHERE @Year BETWEEN StartYear AND LatestYear.
        /// </summary>
        public static string Dml_IsVehicleMakeYearGroupExists {
            get {
                return ResourceManager.GetString("Dml_IsVehicleMakeYearGroupExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT count(1) FROM [RedBook].[VEVehicle_View] WHERE [MakeCode] = @MakeCode and FamilyCode = @FamilyCode.
        /// </summary>
        public static string Dml_VehicleByMakeCodeAndFamilyCodeExists {
            get {
                return ResourceManager.GetString("Dml_VehicleByMakeCodeAndFamilyCodeExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT count(1) FROM [RedBook].[VEVehicle_View] WHERE [MakeCode] = @MakeCode.
        /// </summary>
        public static string Dml_VehicleByMakeCodeExists {
            get {
                return ResourceManager.GetString("Dml_VehicleByMakeCodeExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT count(1) FROM [RedBook].[VEVehicle_View] WHERE [MakeCode] = @MakeCode and FamilyCode = @FamilyCode and YearGroup = @Year
        ///.
        /// </summary>
        public static string Dml_VehicleByMakeCodeFamilyCodeAndYearExists {
            get {
                return ResourceManager.GetString("Dml_VehicleByMakeCodeFamilyCodeAndYearExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT count(1) FROM [RedBook].[VEYear_View] WHERE [MakeCode] = @MakeCode and FamilyCode = @FamilyCode.
        /// </summary>
        public static string Dml_VehicleYearsByMakeCodeAndFamilyCodeExists {
            get {
                return ResourceManager.GetString("Dml_VehicleYearsByMakeCodeAndFamilyCodeExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT count(1) FROM [RedBook].[VEYear_View] WHERE [MakeCode] = @MakeCode.
        /// </summary>
        public static string Dml_VehicleYearsByMakeCodeExists {
            get {
                return ResourceManager.GetString("Dml_VehicleYearsByMakeCodeExists", resourceCulture);
            }
        }
    }
}
