// <copyright file="SamlExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ExtensionMethods
{
    using System.Linq;
    using ComponentSpace.Saml2;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public static class SamlExtensions
    {
        public static string GetAttributeValueOrThrow(this ISpSsoResult ssoResult, string attributeName)
        {
            var attribute = ssoResult.Attributes?.FirstOrDefault(x => x.Name == attributeName);
            if (attribute == null)
            {
                throw new ErrorException(Errors.Authentication.Saml.AttributeNotFound(attributeName));
            }

            string value = attribute.AttributeValues.FirstOrDefault().ToString();
            return value;
        }

        public static string? GetAttributeValueOrNull(this ISpSsoResult ssoResult, string? attributeName)
        {
            if (attributeName == null)
            {
                return null;
            }

            var attribute = ssoResult.Attributes?.FirstOrDefault(x => x.Name == attributeName);
            if (attribute == null)
            {
                return null;
            }

            string value = attribute.AttributeValues.FirstOrDefault().ToString();
            return value;
        }
    }
}
