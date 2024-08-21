// <copyright file="PersonInformationHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers;

using System.Text.RegularExpressions;
using UBind.Domain.Extensions;

/// <summary>
/// A helper class for masking person information.
/// </summary>
public static class PersonInformationHelper
{

    /// <summary>
    /// Mask an email address.
    /// </summary>
    /// <param name="value">The email address to mask.</param>
    /// <returns>The masked email address.</returns>
    public static string GetMaskedEmail(this string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var trimmedValue = value.Trim();
        var match = Regex.Match(trimmedValue, "^([^@]+)@([^@]+)");
        if (match.Success)
        {
            var maskedFirstPart = match.Groups[1].Value.MaskStringLeavingOnlyFirstAndLastCharacters();
            var secondPart = match.Groups[2].Value;
            string maskedSecondPart;
            if (secondPart.Contains('.'))
            {
                var indexOfLastDot = secondPart.LastIndexOf('.');
                var tld = secondPart.Substring(indexOfLastDot);
                var restOfDomain = secondPart.Substring(0, indexOfLastDot);
                maskedSecondPart = restOfDomain.MaskStringLeavingOnlyFirstCharacter() + tld;
            }
            else
            {
                maskedSecondPart = secondPart.MaskStringLeavingOnlyFirstAndLastCharacters();
            }

            return maskedFirstPart + "@" + maskedSecondPart;
        }
        else
        {
            return trimmedValue.MaskStringLeavingOnlyFirstAndLastCharacters();
        }
    }

    /// <summary>
    /// Mask an email address with hashing.
    /// i.e. j***e_987348786843@u***.io.
    /// </summary>
    /// <param name="value">The email address to mask.</param>
    /// <returns>The masked email address.</returns>
    public static string GetMaskedEmailWithHashing(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var token = "***";
        var trimmedValue = value.Trim();
        var match = Regex.Match(trimmedValue, "^([^@]+)@([^@]+)");
        if (match.Success)
        {
            var maskedFirstPart = match.Groups[1].Value.MaskStringLeavingOnlyFirstAndLastCharacters(token: token);
            var hashedEmail = trimmedValue.GetHashString();
            maskedFirstPart = $"{maskedFirstPart}_{(hashedEmail.Length > 8 ? hashedEmail.Substring(0, 8) : hashedEmail)}";
            var secondPart = match.Groups[2].Value;
            string maskedSecondPart;
            if (secondPart.Contains('.'))
            {
                var indexOfLastDot = secondPart.LastIndexOf('.');
                var tld = secondPart.Substring(indexOfLastDot);
                var restOfDomain = secondPart.Substring(0, indexOfLastDot);
                maskedSecondPart = restOfDomain.MaskStringLeavingOnlyFirstAndLastCharacters(token: token) + tld;
            }
            else
            {
                maskedSecondPart = secondPart.MaskStringLeavingOnlyFirstAndLastCharacters(token: token);
            }

            return maskedFirstPart + "@" + maskedSecondPart;
        }
        else
        {
            return trimmedValue.MaskStringLeavingOnlyFirstAndLastCharacters(token: token);
        }
    }

    /// <summary>
    /// Mask a name.
    /// </summary>
    /// <param name="value">The name to mask.</param>
    /// <returns>The masked name.</returns>
    public static string GetMaskedNameWithHashing(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        string token = "***";
        var trimmedValue = value.Trim();
        if (Regex.IsMatch(trimmedValue, "\\s"))
        {
            var hashedName = trimmedValue.GetHashString();
            var maskedName = trimmedValue.MaskStringLeavingOnlyFirstAndLastCharacters(token: token);
            maskedName = $"{maskedName}_{(hashedName.Length > 8 ? hashedName.Substring(0, 8) : hashedName)}";
            return maskedName;
        }

        if (trimmedValue.Length > 0)
        {
            return trimmedValue[0] + token;
        }

        return trimmedValue;
    }
}
