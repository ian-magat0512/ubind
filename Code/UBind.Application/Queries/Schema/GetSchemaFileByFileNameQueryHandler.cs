// <copyright file="GetSchemaFileByFileNameQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Schema
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using UBind.Application.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    public class GetSchemaFileByFileNameQueryHandler : IQueryHandler<GetSchemaFileByFileNameQuery, object>
    {
        private readonly ISchemaConfiguration schemaConfiguration;

        public GetSchemaFileByFileNameQueryHandler(ISchemaConfiguration schemaConfiguration)
        {
            this.schemaConfiguration = schemaConfiguration;
        }

        public async Task<object> Handle(GetSchemaFileByFileNameQuery request, System.Threading.CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var isFileAllowed = this.schemaConfiguration.AllowedFileNames.Contains(request.FileName);
            if (!isFileAllowed)
            {
                throw new ErrorException(Domain.Errors.Schema.NotPublished(request.FileName));
            }

            string fileNameToSearch = request.FileName;
            string schemasFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "schemas");

            string[] filePaths = Directory.GetFiles(schemasFolderPath, request.FileName, SearchOption.AllDirectories);

            // File not found
            if (filePaths.Length == 0)
            {
                throw new FileNotFoundException($"File '{request.FileName}' not found in the 'schemas' directory and its subdirectories.");
            }

            string jsonFilePath = filePaths[0];
            using (StreamReader file = new StreamReader(jsonFilePath))
            {
                string jsonData = await file.ReadToEndAsync();
                return JsonConvert.DeserializeObject(jsonData);
            }
        }
    }
}
