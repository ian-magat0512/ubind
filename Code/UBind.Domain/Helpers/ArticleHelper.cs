// <copyright file="ArticleHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers;

public static class ArticleHelper
{
    public static string GetArticle(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return string.Empty;
        }

        // Convert to lowercase to simplify checks
        word = word.ToLower();

        // Check if the word starts with a vowel sound
        if (word.StartsWith('a') || word.StartsWith('e') || word.StartsWith('i')
            || word.StartsWith('o') || word.StartsWith('u'))
        {
            return "an";
        }

        // Handle specific exceptions (basic examples)
        else if (word.StartsWith("hour") || word.StartsWith("honest"))
        {
            return "an";
        }
        else if (word.StartsWith("uni") && (word.StartsWith("univ") || word.StartsWith("uni-")))
        {
            return "a";
        }

        // Default to "a" for consonant sounds
        else
        {
            return "a";
        }
    }
}
