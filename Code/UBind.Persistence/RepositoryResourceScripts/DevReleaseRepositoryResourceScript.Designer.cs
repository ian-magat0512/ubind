﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UBind.Persistence.RepositoryResourceScripts {
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
    internal class DevReleaseRepositoryResourceScript {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DevReleaseRepositoryResourceScript() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UBind.Persistence.RepositoryResourceScripts.DevReleaseRepositoryResourceScript", typeof(DevReleaseRepositoryResourceScript).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -- Fetch DevRelease with QuoteDetails and ClaimDetails
        ///SELECT TOP(1) dr.*, qd.*, cd.*
        ///FROM DevReleases dr
        ///LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
        ///LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
        ///WHERE dr.TenantId = @TenantId AND dr.Id = @ReleaseId;
        ///
        ///-- Fetch all Assets associated with the ReleaseDetails of a specific Release
        ///WITH ReleaseDetailsIds AS (
        ///    SELECT QuoteDetails_Id AS Id
        ///    FROM DevReleases
        ///    WHERE TenantId = @TenantId AND Id = @ReleaseId
        ///    UNION
        ///   [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetDevReleaseByIdWithFileContents {
            get {
                return ResourceManager.GetString("GetDevReleaseByIdWithFileContents", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -- Fetch DevRelease with QuoteDetails and ClaimDetails
        ///SELECT TOP(1) dr.*, qd.*, cd.*
        ///FROM DevReleases dr
        ///LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
        ///LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
        ///WHERE dr.TenantId = @TenantId AND dr.ProductId = @ProductId;
        ///
        ///-- Fetch Assets for QuoteDetails and ClaimDetails
        ///WITH ReleaseDetailsIds AS (
        ///    SELECT QuoteDetails_Id AS Id
        ///    FROM DevReleases
        ///    WHERE TenantId = @TenantId AND ProductId = @ProductId
        ///    UNION
        ///    SELECT Clai [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetDevReleaseForProductWithoutAssetFileContents {
            get {
                return ResourceManager.GetString("GetDevReleaseForProductWithoutAssetFileContents", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -- Fetch DevRelease with QuoteDetails and ClaimDetails
        ///SELECT TOP(1) dr.*, qd.*, cd.*
        ///FROM DevReleases dr
        ///LEFT JOIN ReleaseDetails qd ON dr.QuoteDetails_Id = qd.Id
        ///LEFT JOIN ReleaseDetails cd ON dr.ClaimDetails_Id = cd.Id
        ///WHERE dr.TenantId = @TenantId AND dr.Id = @ProductReleaseId;
        ///
        ///-- Fetch Assets for QuoteDetails and ClaimDetails
        ///WITH ReleaseDetailsIds AS (
        ///    SELECT QuoteDetails_Id AS Id
        ///    FROM DevReleases
        ///    WHERE TenantId = @TenantId AND Id = @ProductReleaseId
        ///    UNION
        ///    SELECT Clai [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetDevReleaseWithoutAssetFileContents {
            get {
                return ResourceManager.GetString("GetDevReleaseWithoutAssetFileContents", resourceCulture);
            }
        }
    }
}
