// <copyright file="Auth0UserService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

////namespace UBind.Application.Authentication
////{
////    using System;
////    using System.Diagnostics.Contracts;
////    using System.Linq;
////    using System.Text;
////    using System.Threading.Tasks;
////    using Auth0.Core.Exceptions;
////    using Auth0.ManagementApi;
////    using Auth0.ManagementApi.Models;
////    using UBind.Application.User;
////    using UBind.Domain.Exceptions;

////    /// <inheritdoc />
////    public class Auth0UserService : IAuthUserService<User>
////    {
////        private const string AUTH0CONNECTION = "Username-Password-Authentication";

////        private readonly IAuth0Configuration configuration;
////        private readonly IAuth0TokenProvider tokenProvider;

////        /// <summary>
////        /// Initializes a new instance of the <see cref="Auth0UserService"/> class.
////        /// </summary>////        /// <param name="configuration">Auth0 configuration.</param>
////        /// <param name="tokenProvider">Token provider.</param>
////        public Auth0UserService(IAuth0Configuration configuration, IAuth0TokenProvider tokenProvider)
////        {
////            this.configuration = configuration;
////            this.tokenProvider = tokenProvider;
////        }

////        /// <inheritdoc />
////        public async Task<User> GetUserByEmailAsync(string email)
////        {
////            ManagementApiClient client = await this.GetClient();
////            return (await client.Users.GetUsersByEmailAsync(email)).FirstOrDefault();
////        }

////        /// <inheritdoc />
////        public async Task<User> CreateUser(string email, string role)
////        {
////            try
////            {
////                var request = new UserCreateRequest
////                {
////                    Connection = AUTH0CONNECTION,
////                    Email = email,
////                    Password = this.CreatePassword(12),
////                    UserMetadata = new UserMetadata { Role = role }, // TODO: remove
////                    AppMetadata = new { role }
////                };

////                var client = await this.GetClient();
////                return await client.Users.CreateAsync(request);
////            }
////            catch (Exception)
////            {
////                throw new FailedUserCreationException($"Failed to create user {email}.");
////            }
////        }

////        /// <inheritdoc />
////        public async Task<User> UpdateUser(string userId, string email, bool blocked)
////        {
////            try
////            {
////                ManagementApiClient client = await this.GetClient();
////                UserUpdateRequest userUpdateRequest = new UserUpdateRequest
////                {
////                    Email = email,
////                    Blocked = blocked
////                };

////                return await client.Users.UpdateAsync(userId, userUpdateRequest);
////            }
////            catch (ApiException ex)
////            {
////                throw new DuplicateUserEmailException(ex.Message);
////            }
////        }

////        /// <inheritdoc />
////        public async Task<User> SetPassword(string userId, string password)
////        {
////            try
////            {
////                ManagementApiClient client = await this.GetClient();
////                UserUpdateRequest userUpdateRequest = new UserUpdateRequest
////                {
////                    Password = password
////                };

////                return await client.Users.UpdateAsync(userId, userUpdateRequest);
////            }
////            catch (Exception)
////            {
////                throw new FailedUserUpdateException($"Failed to create password for user Id {userId}.");
////            }
////        }

////        private async Task<ManagementApiClient> GetClient()
////        {
////            AccessToken token = await this.tokenProvider.GetAccessToken();
////            return new ManagementApiClient(token.Token, this.configuration.TenantDomain);
////        }

////        private string CreatePassword(int length)
////        {
////            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
////            StringBuilder res = new StringBuilder();
////            Random rnd = new Random();
////            while (length-- > 0)
////            {
////                res.Append(valid[rnd.Next(valid.Length)]);
////            }

////            return res.ToString();
////        }

////        private class UserMetadata
////        {
////            public string FirstName { get; set; }

////            public string LastName { get; set; }

////            public string PhoneNumber { get; set; }

////            public string Role { get; set; }
////        }
////    }
////}
