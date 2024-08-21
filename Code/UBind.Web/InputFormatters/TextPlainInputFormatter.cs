// <copyright file="TextPlainInputFormatter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.InputFormatters
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Formatters;

    /// <summary>
    /// Allows us to take a plain text body as a string value in a controller action.
    /// </summary>
    public class TextPlainInputFormatter : InputFormatter
    {
        private const string ContentType = "text/plain";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlainInputFormatter"/> class.
        /// </summary>
        public TextPlainInputFormatter()
        {
            this.SupportedMediaTypes.Add(ContentType);
        }

        /// <inheritdoc/>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        /// <inheritdoc/>
        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            return contentType != null && contentType.StartsWith(ContentType);
        }
    }
}
