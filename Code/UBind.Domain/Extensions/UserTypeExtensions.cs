// <copyright file="UserTypeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions
{
    using UBind.Domain.Exceptions;

    public static class UserTypeExtensions
    {
        public static PortalUserType ToPortalUserType(this string? userType)
        {
            var enumValue = userType.ToEnumOrThrow<UserType>();
            return ToPortalUserType(enumValue);
        }

        public static PortalUserType ToPortalUserType(this UserType userType)
        {
            switch (userType)
            {
                case UserType.Master:
                case UserType.Client:
                    return PortalUserType.Agent;
                case UserType.Customer:
                    return PortalUserType.Customer;
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(userType, typeof(UserType)));
            }
        }
    }
}
