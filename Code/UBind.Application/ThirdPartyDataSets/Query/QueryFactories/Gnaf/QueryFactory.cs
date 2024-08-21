// <copyright file="QueryFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ThirdPartyDataSets.Query.QueryFactories.Gnaf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;

    /// <inheritdoc/>
    public class QueryFactory : IQueryFactory
    {
        /// <inheritdoc/>
        public string[] CreateQueryFilterFields(string searchQuery)
        {
            var queryParts = searchQuery.Split(' ');

            if (queryParts.Length == 1)
            {
                return new[] { AddressConstants.NumberFirst };
            }

            return new[]
            {
                AddressConstants.FlatType,
                AddressConstants.FlatNumber,
                AddressConstants.LevelNumber,
                AddressConstants.LotNumber,
                AddressConstants.NumberFirst,
                AddressConstants.StreetName,
                AddressConstants.StreetTypeCode,
                AddressConstants.StreetTypeShortName,
                AddressConstants.LocalityName,
                AddressConstants.StateAbbreviation,
                AddressConstants.PostCode,
                AddressConstants.FullAddress,
                AddressConstants.Latitude,
                AddressConstants.Longitude,
            };
        }

        /// <inheritdoc/>
        public string CreateQueryTerms(string rawSearchString)
        {
            var terms = this.ExtractTerms(rawSearchString);

            var resultTerms = new List<string>();

            for (var i = 0; i < terms.Count; i++)
            {
                var term = terms[i];
                var isNumberAndNotFirstTerm = term.IndexOf("*", StringComparison.Ordinal) == -1 && terms.Count > 1 && i > 0;

                if (isNumberAndNotFirstTerm)
                {
                    term += int.TryParse(term, out var dummy) ? "*" : string.Empty;
                }
                else
                {
                    if (!int.TryParse(term, out var num))
                    {
                        var isSingleCharacterAndNextIndexNotOutOfBounds =
                            term.Replace("*", string.Empty).Length == 1 && i + 1 <= terms.Count - 1;
                        if (isSingleCharacterAndNextIndexNotOutOfBounds)
                        {
                            var nextTerm = terms[i + 1];
                            if (!int.TryParse(term, out var num2))
                            {
                                term = (term.IndexOf("*", StringComparison.Ordinal) > -1
                                    ? term.Replace("*", string.Empty)
                                    : term) + nextTerm;
                                terms[i + 1] = string.Empty;
                            }
                        }

                        if (term.Length > 0)
                        {
                            var termWithQuote = string.Concat(term.AsSpan(0, 1), term.IndexOf("'", StringComparison.Ordinal) < 0
                                ? "'"
                                : string.Empty, term.AsSpan(1, term.Length - 1));
                            term = "(" + term + " OR " + termWithQuote + ")";
                        }
                    }
                    else
                    {
                        if (i > 1 && !term.Contains('*'))
                        {
                            term += "*";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(term))
                {
                    resultTerms.Add(term);
                }
            }

            resultTerms = resultTerms.Where(t => t.Length > 0).ToList();

            return string.Join(" AND ", resultTerms);
        }

        private List<string> ExtractTerms(string rawSearchString)
        {
            rawSearchString =
                string.IsNullOrEmpty(rawSearchString) ? string.Empty : rawSearchString.Replace("?", string.Empty);
            var terms = rawSearchString.Trim()
                .ToLower().Replace("-", " ")
                .Replace("/", " ")
                .Replace(",", " ")
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim() + (int.TryParse(x, out var foo) ? string.Empty : "*")).ToList();
            return terms;
        }
    }
}
