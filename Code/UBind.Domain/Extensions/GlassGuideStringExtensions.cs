// <copyright file="GlassGuideStringExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions;

public static class GlassGuideStringExtensions
{
    private const int MaxWordLength = 4;

    /// <summary>
    /// Glass's Guide words that will be converted to title case regardless of length.
    /// </summary>
    private static string[] glassGuideSpecialWords = new string[]
    {
        "AND", "VAN", "BUS", "TOW", "TOP", "AIR", "CAB", "BOX", "TRI", "BI",
        "ONE", "TWO", "SIX", "TEN", "INJ", "DIR", "DE", "REO", "SAT", "MAX",
        "SEQ", "DIR", "MAN", "KIT", "NAV",
    };

    /// <summary>
    /// Delimiter characters that seperate words in a vehicle data description.
    /// </summary>
    private static char[] delimiterChars = new char[]
    {
        '-', '+', '/', '(', ')',
    };

    /// <summary>
    /// Converts a description from vehicle data into title case following Glass's Guide specifications.
    /// </summary>
    /// <param name="value">The string to be converted.</param>
    /// <returns>The result string.</returns>
    public static string ToGlassGuideVehicleDescription(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Length < MaxWordLength)
        {
            return glassGuideSpecialWords.Contains(value, StringComparer.OrdinalIgnoreCase) ? value.ToLower().ToUpperFirstChar() : value;
        }

        string word = string.Empty;
        string result = string.Empty;
        for (int i = 0; i < value.Length; ++i)
        {
            char ch = value[i];
            if (delimiterChars.Contains(ch) || char.IsWhiteSpace(ch))
            {
                if (!string.IsNullOrEmpty(word))
                {
                    result += word.ToTitleCaseIfHasLettersOnlyWithVowel();
                    word = string.Empty;
                }
                result += ch;
            }
            else
            {
                word += ch;
            }
        }

        if (!string.IsNullOrEmpty(word))
        {
            result += word.ToTitleCaseIfHasLettersOnlyWithVowel();
        }

        return result;
    }
}
