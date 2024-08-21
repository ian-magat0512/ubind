CREATE TRIGGER ClaimReadModelsCopyQuoteIdIntoPolicyIdOnInsert
   ON dbo.ClaimReadModels
   AFTER INSERT
AS 
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.ClaimReadModels
    SET QuoteId = i.PolicyId
    FROM inserted i
        INNER JOIN dbo.ClaimReadModels t ON i.Id = t.Id
    WHERE i.PolicyId IS NOT NULL
END