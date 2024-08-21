// <copyright file="FixInvalidQuestionSetAttachmentsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Migration;

using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class FixInvalidQuestionSetAttachmentsCommandHandler
    : ICommandHandler<FixInvalidQuestionSetAttachmentsCommand, Unit>
{
    private readonly IUBindDbContext dbContext;
    private readonly ILogger<FixCorruptPolicyDocumentsCommandHandler> logger;
    private readonly IBackgroundJobClient backgroundJobClient;

    public FixInvalidQuestionSetAttachmentsCommandHandler(
        IUBindDbContext dbContext,
        ILogger<FixCorruptPolicyDocumentsCommandHandler> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.backgroundJobClient = backgroundJobClient;
    }

    public Task<Unit> Handle(FixInvalidQuestionSetAttachmentsCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => this.HandleCancellationRequest());
        this.backgroundJobClient.Enqueue(() => this.FixInvalidQuestionSetAttachments(request.AttachmentNames, cancellationToken));
        return Task.FromResult(Unit.Value);
    }

    [JobDisplayName("Fix Question Set Attachments With Invalid Attachment Ids")]
    public async Task FixInvalidQuestionSetAttachments(string[] attachmentNames, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            this.HandleCancellationRequest();
        }

        this.logger.LogInformation($"Started at {DateTime.Now}");
        var dbTimeout = this.dbContext.Database.CommandTimeout;

        using (var transaction = this.dbContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
        {
            try
            {
                this.dbContext.Database.CommandTimeout = 0;
                foreach (var filename in attachmentNames)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        this.HandleCancellationRequest();
                    }

                    this.logger.LogInformation($"Processing for '{filename}' attachments");
                    string sql = GetSqlToFixInvalidQuestionSetAttachments();
                    this.logger.LogInformation(sql);
                    var sqlParam = new SqlParameter("@FileName", filename);
                    await this.dbContext.Database.ExecuteSqlCommandAsync(sql, sqlParam);

                    this.logger.LogInformation($"Updated '{filename}' with valid attachment Ids.");
                    await Task.Delay(500, cancellationToken);
                }

                this.logger.LogInformation($"Completed at {DateTime.Now}.");
                transaction.Commit();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                transaction.Rollback();
            }
            finally
            {
                this.dbContext.Database.CommandTimeout = dbTimeout;
            }
        }
    }

    private static string GetSqlToFixInvalidQuestionSetAttachments()
    {
        return $@"
DECLARE @QuoteId UNIQUEIDENTIFIER
DECLARE @AttachmentInfo NVARCHAR(500)
DECLARE @NewAttachmentInfo NVARCHAR(500)
DECLARE @FormAttachmentId UNIQUEIDENTIFIER
DECLARE @ExistingAttachmentId UNIQUEIDENTIFIER
DECLARE @QuoteCounter INT = 1


DROP TABLE IF EXISTS #QuoteFormAttachment
CREATE TABLE #QuoteFormAttachment (
	Id int identity,
	QuoteId uniqueidentifier,
	AttachmentInfo varchar(500)
)

INSERT INTO #QuoteFormAttachment
	SELECT
		QuoteId = Q.Id,
		AttachmentInfo = SUBSTRING(
						LatestFormData,
						CHARINDEX('""'+@FileName+':', LatestFormData) + 1,
						CHARINDEX('""', LatestFormData,CHARINDEX('""'+@FileName+':', LatestFormData)+1) - CHARINDEX('""'+@FileName+':', LatestFormData)-1)
	FROM Quotes Q
	WHERE Q.LatestFormData LIKE '%""'+@FileName+':%'

SELECT @QuoteId = QuoteId,
	@AttachmentInfo = AttachmentInfo
FROM #QuoteFormAttachment WHERE ID = @QuoteCounter

WHILE @QuoteId IS NOT NULL
BEGIN
	/* Get the attachment filename and attachment id from the form data*/
	/* Attachment Question Field Format -- ""File Name:File Type:Attachment Id:"" */
	SELECT @FormAttachmentId = SUBSTRING(@AttachmentInfo, 
									CHARINDEX(':', @AttachmentInfo, CHARINDEX(':', @AttachmentInfo) + 1) + 1,
									CHARINDEX(':', @AttachmentInfo, CHARINDEX(':', @AttachmentInfo, CHARINDEX(':', @AttachmentInfo) +1) +1)  - CHARINDEX(':', @AttachmentInfo, CHARINDEX(':', @AttachmentInfo) + 1) - 1
								)

	/* Get the latest Id with the same name */
	SELECT TOP 1
		@ExistingAttachmentId = Id
	FROM QuoteFileAttachments
	WHERE QuoteId = @QuoteId 
		AND Name = @FileName 
	ORDER BY CreatedTicksSinceEpoch DESC

	/* Update the formData if attachment Id's doesn't match in the QuoteFileAttachments and formData */
	IF (@ExistingAttachmentId <> @FormAttachmentId)
	BEGIN
		SET @NewAttachmentInfo = REPLACE(@AttachmentInfo, @FormAttachmentId, LOWER(@ExistingAttachmentId))
		UPDATE Quotes 
			SET LatestFormData = REPLACE(LatestFormData, '""'+@AttachmentInfo+'""', '""'+@NewAttachmentInfo+'""'),
				SerializedLatestCalculationResult = REPLACE(SerializedLatestCalculationResult, '\""'+@AttachmentInfo+'\""', '\""'+@NewAttachmentInfo+'\""')
		WHERE Id = @QuoteId
	END
		
	SET @ExistingAttachmentId = NULL
	SET @QuoteId = NULL
	SET @AttachmentInfo = NULL
	SET @QuoteCounter += 1

	SELECT @QuoteId = QuoteId,
		@AttachmentInfo = AttachmentInfo
	FROM #QuoteFormAttachment WHERE ID = @QuoteCounter
END

DROP TABLE IF EXISTS #QuoteFormAttachment
";
    }

    private void HandleCancellationRequest()
    {
        this.logger.LogInformation("FixInvalidQuestionSetAttachments job was canceled.");
        throw new OperationCanceledException();
    }
}
