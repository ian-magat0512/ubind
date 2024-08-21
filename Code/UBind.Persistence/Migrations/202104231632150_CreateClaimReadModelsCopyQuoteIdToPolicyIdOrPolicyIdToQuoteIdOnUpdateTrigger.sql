CREATE TRIGGER ClaimReadModelsCopyQuoteIdToPolicyIdOrPolicyIdToQuoteIdOnUpdate
   ON dbo.ClaimReadModels
   AFTER UPDATE
AS 
BEGIN
    SET NOCOUNT ON;

	IF UPDATE(PolicyId)
	BEGIN
		UPDATE dbo.ClaimReadModels
		SET QuoteId = i.PolicyId
		FROM inserted i
			INNER JOIN dbo.ClaimReadModels t ON i.Id = t.Id
	END
	IF UPDATE(QuoteId)
	BEGIN
		UPDATE dbo.ClaimReadModels
		SET PolicyId = i.QuoteId
		FROM inserted i
			INNER JOIN dbo.ClaimReadModels t ON i.Id = t.Id
	END
END