CREATE TRIGGER dbo.SyncCustomerPrimaryPersonIdAndPersonIdColumnsOnUpsert
   ON dbo.CustomerReadModels
   AFTER INSERT, UPDATE
AS 
BEGIN
    SET NOCOUNT ON;
	IF COL_LENGTH('dbo.CustomerReadModels', 'PrimaryPersonId') IS NOT NULL
	BEGIN
		DECLARE @CustomerId UNIQUEIDENTIFIER
		SELECT @CustomerId = INSERTED.Id FROM INSERTED
	
		UPDATE dbo.CustomerReadModels SET PrimaryPersonId = PersonId
			WHERE Id = @CustomerId
			AND PrimaryPersonId <> PersonId
			AND PersonId <> '00000000-0000-0000-0000-000000000000'
			AND PrimaryPersonId = '00000000-0000-0000-0000-000000000000';
	END
END
