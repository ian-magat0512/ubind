// <copyright file="FileUploadOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// This filter is used to implement the option to modify or replace the parameters made for file upload requests to be able to be used
    /// in swagger.
    /// </summary>
    public class FileUploadOperation : IOperationFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.OperationId?.ToLower() == "apiimportcsvfileimportpost")
            {
                operation.Parameters.Clear();
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "uploadedFile",
                    In = ParameterLocation.Header,
                    Description = "Upload File",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "file",
                        Format = "binary",
                    },
                });

                var uploadFileMediaType = new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "object",
                        Properties =
                        {
                            ["uploadedFile"] = new OpenApiSchema()
                            {
                                Description = "Upload File",
                                Type = "file",
                                Format = "binary",
                            },
                        },
                        Required = new HashSet<string>()
                        {
                            "uploadedFile",
                        },
                    },
                };
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = uploadFileMediaType,
                    },
                };
            }
        }
    }
}
