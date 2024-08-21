// <copyright file="EnumSchemaFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Swagger;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Runtime.Serialization;

/// <summary>
/// Ensures that enums are displayed as strings in Swagger UI, so a droplist can be shown from the options.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            model.Enum.Clear();
            foreach (string enumName in Enum.GetNames(context.Type))
            {
                System.Reflection.MemberInfo memberInfo
                    = context.Type.GetMember(enumName).FirstOrDefault(m => m.DeclaringType == context.Type);
                EnumMemberAttribute enumMemberAttribute = memberInfo == null
                    ? null
                    : memberInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false).OfType<EnumMemberAttribute>().FirstOrDefault();
                string label = enumMemberAttribute == null || string.IsNullOrWhiteSpace(enumMemberAttribute.Value)
                    ? enumName
                    : enumMemberAttribute.Value;
                model.Enum.Add(new OpenApiString(label));
            }
        }
    }
}
