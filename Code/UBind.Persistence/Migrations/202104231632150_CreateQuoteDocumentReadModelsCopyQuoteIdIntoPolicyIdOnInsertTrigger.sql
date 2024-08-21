CREATE TRIGGER QuoteDocumentReadModelsCopyQuoteIdIntoPolicyIdOnInsert
   ON dbo.QuoteDocumentReadModels
   AFTER INSERT
AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE  dbo.QuoteDocumentReadModels
    SET QuoteId = i.PolicyId
    FROM inserted i
        INNER JOIN dbo.QuoteDocumentReadModels t ON i.Id = t.Id
    WHERE i.PolicyId IS NOT NULL
END