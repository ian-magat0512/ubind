CREATE TRIGGER ClaimReadModelsCopyPolicyIdIntoQuoteIdOnInsert
   ON dbo.ClaimReadModels
   AFTER INSERT
AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ClaimReadModels
    SET PolicyId = i.QuoteId
    FROM inserted i
        INNER JOIN dbo.ClaimReadModels t ON i.Id = t.Id
    WHERE i.QuoteId IS NOT NULL
END