// <copyright file="WebAddress.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// This class is needed to represent Website addresses.
    /// </summary>
    public class WebAddress : ValueObject
    {
        private readonly string webAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAddress"/> class.
        /// </summary>
        /// <param name="uri">The web address.</param>
        public WebAddress(string uri)
        {
            this.ValidateWebAddress(uri);
            this.webAddress = uri;
        }

        /// <summary>
        /// Method to override default ToString method.
        /// </summary>
        /// <returns>The web address.</returns>
        public override string ToString()
        {
            return this.webAddress.ToString();
        }

        /// <summary>
        /// Method for overriding the GetEqualityComponents method.
        /// </summary>
        /// <returns>The list of equality components.</returns>
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.webAddress;
        }

        private void ValidateWebAddress(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ErrorException(Errors.WebAddress.WebAddressInvalid(value));
            }

            const string webAddressExpression = @"^(?:(?:(?:https?|ftp):)?\/\/)?(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?";
            Regex regex = new Regex(webAddressExpression);
            Match match = regex.Match(value.ToString());

            if (!match.Success)
            {
                throw new ErrorException(Errors.WebAddress.WebAddressInvalid(value));
            }
        }
    }
}
