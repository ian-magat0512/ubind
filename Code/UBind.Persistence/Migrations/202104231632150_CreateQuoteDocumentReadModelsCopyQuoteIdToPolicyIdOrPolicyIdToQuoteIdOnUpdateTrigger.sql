CREATE TRIGGER QuoteDocumentReadModelsCopyQuoteIdToPolicyIdOrPolicyIdToQuoteIdOnUpdate
   ON dbo.QuoteDocumentReadModels
   AFTER UPDATE
AS 
BEGIN
    SET NOCOUNT ON;

	IF UPDATE(PolicyId)
	BEGIN
		UPDATE dbo.QuoteDocumentReadModels
		SET QuoteId = i.PolicyId
		FROM inserted i
			INNER JOIN dbo.QuoteDocumentReadModels t ON i.Id = t.Id
	END
	IF UPDATE(QuoteId)
	BEGIN
		UPDATE dbo.QuoteDocumentReadModels
		SET PolicyId = i.QuoteId
		FROM inserted i
			INNER JOIN dbo.QuoteDocumentReadModels t ON i.Id = t.Id
	END
END