CREATE TRIGGER QuoteDocumentReadModelsCopyPolicyIdIntoQuoteIdOnInsert
   ON dbo.QuoteDocumentReadModels
   AFTER INSERT
AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE  dbo.QuoteDocumentReadModels
    SET PolicyId = i.QuoteId
    FROM inserted i
        INNER JOIN dbo.QuoteDocumentReadModels t ON i.Id = t.Id
    WHERE i.QuoteId IS NOT NULL
END