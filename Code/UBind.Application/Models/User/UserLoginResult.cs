// <copyright file="UserLoginResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models.User
{
    using System.IdentityModel.Tokens.Jwt;
    using UBind.Domain.ReadModel.User;

    public class UserLoginResult
    {
        public UserReadModel User { get; set; }

        public JwtSecurityToken JwtToken { get; set; }

        public string ReturnUrl { get; set; }
    }
}
