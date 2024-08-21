// <copyright file="AuthenticationMethodModelBinder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Organisation;

    public class AuthenticationMethodModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string body;
            using (var sr = new StreamReader(bindingContext.HttpContext.Request.Body))
            {
                body = sr.ReadToEndAsync().Result;
            }

            var jObject = JObject.Parse(body);
            var typeName = jObject.Value<string>("typeName");
            var type = typeName.ToEnumOrThrow<AuthenticationMethodType>();
            AuthenticationMethodUpsertModel model;
            switch (type)
            {
                case AuthenticationMethodType.Saml:
                    model = jObject.ToObject<SamlAuthenticationMethodUpsertModel>();
                    break;
                case AuthenticationMethodType.LocalAccount:
                    model = jObject.ToObject<LocalAccountAuthenticationMethodUpsertModel>();
                    break;
                /*
                case "OpenID":
                    model = jObject.ToObject<OpenIdAuthenticationMethodUpsertModel>();
                    break;
                */
                default:
                    throw new ErrorException(Errors.General.BadRequest($"A value of \"{typeName}\" was provided for "
                        + "typeName, however that was not one of the values we can process."));
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }
}
