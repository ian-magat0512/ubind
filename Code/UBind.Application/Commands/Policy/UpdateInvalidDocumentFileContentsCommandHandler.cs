// <copyright file="UpdateInvalidDocumentFileContentsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class UpdateInvalidDocumentFileContentsCommandHandler : ICommandHandler<UpdateInvalidDocumentFileContentsCommand>
    {
        private readonly IUBindDbContext dbContext;

        public UpdateInvalidDocumentFileContentsCommandHandler(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<Unit> Handle(UpdateInvalidDocumentFileContentsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // This script updates the FileContents for invalid FTA documents using the FileContentId from the DocumentFiles table
            var sql = $@"WITH cte
AS (SELECT dc.FileContentId, dc.Name, ea.DocumentFile_Id 
FROM Emails e
INNER JOIN EmailAttachments ea on ea.EmailId=e.Id
INNER JOIN DocumentFiles dc on dc.Id=ea.DocumentFile_Id
WHERE e.Id='{request.EmailId}')
UPDATE qd
SET qd.FileContentId=cte.FileContentId
FROM QuoteDocumentReadModels qd
INNER JOIN cte ON cte.Name=qd.Name
WHERE qd.PolicyId='{request.PolicyId}'";
            this.dbContext.Database.ExecuteSqlCommand(sql);
            return Task.FromResult(Unit.Value);
        }
    }
}
