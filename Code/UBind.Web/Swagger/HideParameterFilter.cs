// <copyright file="HideParameterFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Swagger;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Hides specific controller action parameters from swagger documentation.
/// This is so that it will not be used anymore but is still kept for backward compatibility.
/// </summary>
public class HideParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Example condition: Check if the operation is a specific action in a specific controller
        if (context.MethodInfo.DeclaringType.Name == "RedBookVehicleTypesController" &&
            context.MethodInfo.Name == "GetVehicleFamilies")
        {
            var parameterToRemove = operation.Parameters?.SingleOrDefault(p => p.Name.ToLower() == "makecode");
            if (parameterToRemove != null)
            {
                operation.Parameters.Remove(parameterToRemove);
            }
        }
    }
}
