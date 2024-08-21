// <copyright file="WorkbookParserHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper for when parsing the workbook, to fix up icon values.
    /// </summary>
    public class WorkbookParserHelper
    {
        /// <summary>
        /// Adds classes for the relevant icon set.
        /// For example, if you specify the "fa-digging" icon class, you also need the "fa" class for fontawesome to
        /// be able to detect it.
        /// If you specify the "glyphicon-truck" class, you also need the "glphyicon" class for the glyphicon library
        /// to be able to detect it.
        /// This helper ensures that the necessary classes are added to the icon string.
        /// </summary>
        /// <param name="icon">The string of classes to specify the icon to be rendered.</param>
        /// <returns>The string of classes which now includes the icon set class.</returns>
        public static string AddIconSetClasses(string icon)
        {
            string result = icon;

            // add the fa class as necessary
            var faPattern = new Regex("fa. ");
            if (icon.Contains("fa-") && !faPattern.IsMatch(icon))
            {
                result = $"fa {result}";
            }

            // add the glyphicon class as necessary
            if (icon.Contains("glyphicon-") && !icon.Contains("glyphicon "))
            {
                result = $"glyphicon {result}";
            }

            return result;
        }

        /// <summary>
        /// Replace slash quote with just a quote because in html fields in the workbook, that's what people have been entering.
        /// </summary>
        /// <param name="html">The html to fix.</param>
        /// <returns>The html fixed.</returns>
        public static string ReplaceSlashQuoteWithQuoteInHtml(string html)
        {
            return html.Replace(@"\""", @"""").Replace(@"\\'", @"\'");
        }
    }
}
