// <copyright file="Url.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System.Text.RegularExpressions;
    using UBind.Domain.Exceptions;

    public class Url : ValueObject
    {
        /// <summary>
        /// This regex is designed to match the parts of a URL
        ///  (scheme, optional wildcard subdomain, domain or localhost,
        ///  optional port, optional path, and optional query parameters)
        ///  using character sets that conform to standard URL characters
        ///  as defined in RFC 3986 and other relevant specifications.
        /// </summary>
        private static readonly Regex RegexUrlPattern =
            new Regex(
                @"^(?:https?:\/\/)?(?:\*\.)?(localhost|[a-zA-Z0-9.-]+\.[a-zA-Z0-9.-]+)" + // scheme and domain
                @"(?::\d{1,5})?" + // port
                @"(?:\/[a-zA-Z0-9-._~%!$&'()*+,;=:@]*)*" + // path
                @"(?:\?[a-zA-Z0-9-._~%!$&'()*+,;=:@/?]*)?$", // query parameters
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly string url;

        public Url(string url)
        {
            if (!this.IsValidUrl(url))
            {
                throw new ErrorException(Errors.General.UrlInvalid(url));
            }

            this.url = url;
        }

        public static implicit operator Url(string url)
        {
            return new Url(url);
        }

        public static explicit operator string(Url url)
        {
            return url.ToString();
        }

        public override string ToString()
        {
            return this.url;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.url;
        }

        private bool IsValidUrl(string url)
        {
            if (!url.ToLower().StartsWith("http://")
                && !url.ToLower().StartsWith("https://"))
            {
                return false;
            }

            if (RegexUrlPattern.IsMatch(url))
            {
                return true;
            }
            return false;
        }
    }
}
