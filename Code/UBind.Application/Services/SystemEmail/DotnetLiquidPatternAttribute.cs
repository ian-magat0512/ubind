﻿// <copyright file="DotnetLiquidPatternAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// A Validation attribute for validating dotnet liquid syntax.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class DotnetLiquidPatternAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotnetLiquidPatternAttribute"/> class.
        /// </summary>
        public DotnetLiquidPatternAttribute()
        {
        }

        /// <summary>
        /// Validate if a string is a valid dotent liquid syntax.
        /// </summary>
        /// <param name="dotnetLiquidSyntax">The Dotnet Liquid Syntax to validate.</param>
        /// <returns>Boolean.</returns>
        public override bool IsValid(object dotnetLiquidSyntax)
        {
            if (dotnetLiquidSyntax == null)
            {
                return true;
            }

            try
            {
                var template = DotLiquid.Template.Parse(dotnetLiquidSyntax.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
