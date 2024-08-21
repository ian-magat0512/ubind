/****** Script for Populating the missing claim reference field in ClaimReadModel table.  ******/
DECLARE @alphabet AS VARCHAR(26) = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
DECLARE @LastMaxNumber AS INT
DECLARE @ReferenceNo AS VARCHAR(6)

DECLARE @claimId AS UNIQUEIDENTIFIER
DECLARE @tenantId AS VARCHAR(max)
DECLARE @productId AS VARCHAR(max)
DECLARE @environmentId AS INT
DECLARE @usecase AS INT = 2 --claims

DECLARE @offset as BIGINT = 154457888
DECLARE @coprime as BIGINT = 40000003
DECLARE @setsize as BIGINT = 308915776


    DECLARE claim_cursor CURSOR FOR   
    SELECT id, tenantId, productId, environment  
    FROM [dbo].[ClaimReadModels]
    WHERE ClaimReference = ''  OR ClaimReference IS NULL
  
    OPEN claim_cursor  
    FETCH NEXT FROM claim_cursor INTO @claimId, @tenantId, @productId, @environmentId

    WHILE @@FETCH_STATUS = 0  
    BEGIN  
	    
		IF NOT EXISTS(SELECT 1 FROM [dbo].[ReferenceNumberSequences] WHERE tenantId = @tenantId and productId =@productId and Environment=@environmentId and useCase= @usecase)
		BEGIN
		    INSERT INTO [dbo].[ReferenceNumberSequences] SELECT @tenantId, @productId, @environmentId,0, 0,@usecase
		END

		SELECT @LastMaxNumber = MaxNumber,
			   @ReferenceNo = CONCAT(
					SUBSTRING(@alphabet,(((((((sequenceNumber / 26)/26)/26)/26)/26)% 26)+1),1),
					SUBSTRING(@alphabet,((((((sequenceNumber / 26)/26)/26)/26)% 26)+1), 1),
					SUBSTRING(@alphabet,(((((sequenceNumber / 26)/26)/26)% 26)+1),1),
					SUBSTRING(@alphabet,((((sequenceNumber / 26)/26)% 26)+1), 1),
					SUBSTRING(@alphabet,(((sequenceNumber / 26)% 26)+1), 1),
					SUBSTRING(@alphabet,(sequenceNumber % 26)+1, 1))
		FROM
		(
			SELECT MAX([Number]) + 1 AS MaxNumber, ((@offset + (MAX([Number] + 1)* @coprime)) % @setsize)  AS sequenceNumber
			FROM [dbo].[ReferenceNumberSequences]
			WHERE tenantId = @tenantId and productId =@productId and Environment=@environmentId and useCase= @usecase
		) A

		PRINT CAST(@LastMaxNumber AS VARCHAR(50)) + '----' + @ReferenceNo

		BEGIN TRANSACTION;
		BEGIN TRY 
			UPDATE [dbo].[ClaimReadModels] SET ClaimReference = @ReferenceNo WHERE id = @claimId
			INSERT INTO [dbo].[ReferenceNumberSequences] SELECT @tenantId, @productId, @environmentId,0, @LastMaxNumber,@usecase
		END TRY  
		BEGIN CATCH 
				ROLLBACK;  
		END CATCH;  
	    COMMIT; 

        FETCH NEXT FROM claim_cursor INTO @claimId, @tenantId, @productId, @environmentId
    END  
  
    CLOSE claim_cursor  
    DEALLOCATE claim_cursor