/**
Script is for fixing erroneous records in the policy transactions table whose:
1: discriminator column is MidTermAdjustmentTransaction
2: effectiviteTimeInTicksSinceEpoch column is 0
*/
DECLARE @policyTransactionId AS UNIQUEIDENTIFIER
DECLARE @transactionFormData AS VARCHAR(MAX)
DECLARE @discriminator AS VARCHAR(50)
DECLARE @policyInceptionTimeInTicks as BIGINT
DECLARE @adjustmentDateAsDateTime AS DATETIME
DECLARE @adjustmentDateAsTicks AS BIGINT
DECLARE @adjustmentDateAsTicksAdjusted AS BIGINT
DECLARE @epoch AS DATETIME

-- Constant value for start of epoch
SET @epoch = '1970-01-01 00:00:00'

DECLARE transaction_cursor CURSOR FOR SELECT Id, PolicyData_FormData, Discriminator, PolicyData_InceptionTimeInTicksSinceEpoch FROM PolicyTransactions WHERE EffectiveTimeInTicksSinceEpoch = 0
OPEN transaction_cursor

PRINT 'STARTING..'
FETCH NEXT FROM transaction_cursor INTO @policyTransactionId, @transactionFormData, @discriminator, @policyInceptionTimeInTicks
WHILE @@FETCH_STATUS = 0
BEGIN
	IF (@discriminator = 'MidTermAdjustmentTransaction')
	BEGIN
		SET @adjustmentDateAsDateTime = ''
		IF (ISJSON(@transactionFormData) = 1)
		BEGIN		
			BEGIN TRANSACTION;
			BEGIN TRY
				SET @adjustmentDateAsDateTime = CONVERT(DATETIME, JSON_VALUE(@transactionFormData, '$.formModel.policyAdjustmentDate'), 103)
				SET @adjustmentDateAsTicks = CONVERT(BIGINT, DATEDIFF_BIG(MCS, @epoch, @adjustmentDateAsDateTime)) * 10
				SET @adjustmentDateAsTicksAdjusted = @adjustmentDateAsTicks + 216000000000 
				UPDATE PolicyTransactions SET EffectiveTimeInTicksSinceEpoch = @adjustmentDateAsTicksAdjusted WHERE Id = @policyTransactionId
				PRINT 'MIDTERM ADJUSTMENT. VALUE: ' + CAST(@adjustmentDateAsTicksAdjusted AS VARCHAR(25)) + ' ------ ' + CAST(@policyTransactionId AS VARCHAR(50))
			END TRY
			BEGIN CATCH
				ROLLBACK;
			END CATCH
			COMMIT;
		END	
	END
	ELSE
	BEGIN		
		UPDATE PolicyTransactions SET EffectiveTimeInTicksSinceEpoch = @policyInceptionTimeInTicks WHERE Id = @policyTransactionId
		PRINT @discriminator + '. VALUE: ' + CAST(@policyInceptionTimeInTicks AS VARCHAR(25)) + ' ------ ' + CAST(@policyTransactionId AS VARCHAR(50))	
	END
	
	FETCH NEXT FROM transaction_cursor INTO @policyTransactionId, @transactionFormData, @discriminator, @policyInceptionTimeInTicks
END
CLOSE transaction_cursor
DEALLOCATE transaction_cursor